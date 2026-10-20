using EcommerceBackend.Application.Abstractions.Messaging;
using EcommerceBackend.Application.Common;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.IntegrationEvents;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Application.Orders;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using EcommerceBackend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EcommerceBackend.Application.Services;

public class OrderService : IOrderService
{
    public const string DemoNextAction = "DEMO_ADVANCE_FULFILLMENT";
    private const string DefaultCarrier = "Yurtiçi Kargo";
    private const int DefaultAdminPageSize = 20;

    private readonly ApplicationDbContext _context;
    private readonly IOrderRepository _orderRepository;
    private readonly IIntegrationEventPublisher _integrationEvents;
    private readonly ICartService _cartService;
    private readonly ICheckoutPaymentSimulator _paymentSimulator;
    private readonly CheckoutOptions _checkoutOptions;

    public OrderService(
        ApplicationDbContext context,
        IOrderRepository orderRepository,
        IIntegrationEventPublisher integrationEvents,
        ICartService cartService,
        ICheckoutPaymentSimulator paymentSimulator,
        IOptions<CheckoutOptions> checkoutOptions)
    {
        _context = context;
        _orderRepository = orderRepository;
        _integrationEvents = integrationEvents;
        _cartService = cartService;
        _paymentSimulator = paymentSimulator;
        _checkoutOptions = checkoutOptions.Value;
    }

    public async Task<BaseResponseDto<List<OrderDto>>> GetUserOrdersAsync(int userId)
    {
        var orders = await _orderRepository.GetUserOrdersAsync(userId);
        return BaseResponseDto<List<OrderDto>>.SuccessResult("Siparişler listelendi", orders.Select(ToDto).ToList());
    }

    public async Task<BaseResponseDto<OrderDto>> GetOrderByIdAsync(int orderId, int userId)
    {
        var order = await FindOwnedOrderAsync(orderId, userId);
        return order == null
            ? OrderNotFound<OrderDto>()
            : BaseResponseDto<OrderDto>.SuccessResult("Sipariş getirildi", ToDto(order));
    }

    /// <summary>Sipariş oluşturma akışı (§5.2).</summary>
    public async Task<BaseResponseDto<OrderDto>> CreateOrderAsync(int userId, CreateOrderDto createOrderDto, string? idempotencyKey)
    {
        // 1. Idempotency-Key: kırpılır, küçük harfe çevrilir; aynı kullanıcı + anahtar → ilk sipariş 200 ile döner.
        string? key = null;
        if (idempotencyKey != null)
        {
            key = idempotencyKey.Trim().ToLowerInvariant();
            if (key.Length is < 8 or > 128)
                return BaseResponseDto<OrderDto>.Fail("Idempotency-Key 8–128 karakter olmalıdır.", ErrorCodes.IdempotencyKeyInvalid);

            var existing = await _orderRepository.GetByIdempotencyKeyAsync(userId, key);
            if (existing != null)
                return IdempotentReplay(existing);
        }

        // 2. Kalemler ürüne göre birleştirilir.
        var lines = createOrderDto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => (ProductId: g.Key, Quantity: g.Sum(x => x.Quantity)))
            .OrderBy(l => l.ProductId)
            .ToList();
        if (lines.Count == 0)
            return BaseResponseDto<OrderDto>.Fail("Sepette ürün yok.", ErrorCodes.EmptyOrder);

        // 3–4. Adres ve ödeme yöntemi kullanıcıya ait ve aktif olmalı.
        var shippingAddress = await _context.Addresses.FirstOrDefaultAsync(a =>
            a.Id == createOrderDto.ShippingAddressId && a.UserId == userId && a.IsActive);
        if (shippingAddress == null)
            return BaseResponseDto<OrderDto>.Fail("Teslimat adresi geçersiz veya size ait değil.", ErrorCodes.InvalidAddress);

        var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(pm =>
            pm.Id == createOrderDto.PaymentMethodId && pm.UserId == userId && pm.IsActive);
        if (paymentMethod == null)
            return BaseResponseDto<OrderDto>.Fail("Ödeme yöntemi geçersiz veya size ait değil.", ErrorCodes.InvalidPayment);

        // 5. Sunucu sepeti doluysa ödeme özeti onunla birebir eşleşmeli.
        var cartError = await ValidateAgainstCartAsync(userId, lines);
        if (cartError != null)
            return cartError;

        // 6. Tek transaction: stok, ödeme simülasyonu, sipariş, sepet temizliği, bildirim ve olay (outbox).
        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            var (order, error) = await strategy.ExecuteAsync(() =>
                CheckoutAsync(userId, createOrderDto.Notes, key, lines, shippingAddress, paymentMethod));
            if (error != null)
                return error;

            var created = await _orderRepository.GetOrderByIdAsync(order!.Id);
            return BaseResponseDto<OrderDto>.SuccessResult("Siparişiniz alındı", ToDto(created!));
        }
        catch (DbUpdateException ex) when (key != null && ex is not DbUpdateConcurrencyException)
        {
            // Aynı anahtarla eşzamanlı iki istek: benzersiz indeks ikincisini reddeder.
            _context.ChangeTracker.Clear();
            if (await _context.Orders.AnyAsync(o => o.UserId == userId && o.IdempotencyKey == key))
            {
                return BaseResponseDto<OrderDto>.Fail(
                    "Bu Idempotency-Key ile bir sipariş zaten işleniyor. Tekrar deneyin.",
                    ErrorCodes.IdempotencyConflict,
                    409);
            }

            throw;
        }
    }

    private async Task<(Order? Order, BaseResponseDto<OrderDto>? Error)> CheckoutAsync(
        int userId,
        string? notes,
        string? idempotencyKey,
        IReadOnlyList<(int ProductId, int Quantity)> lines,
        Address shippingAddress,
        PaymentMethod paymentMethod)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var productIds = lines.Select(l => l.ProductId).ToList();
        var products = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var now = DateTime.UtcNow;
        var order = new Order
        {
            OrderNumber = GenerateOrderNumber(),
            UserId = userId,
            Status = OrderStatuses.Pending,
            ShippingAddressId = shippingAddress.Id,
            BillingAddressId = shippingAddress.Id,
            PaymentMethodId = paymentMethod.Id,
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            IdempotencyKey = idempotencyKey,
            CreatedAt = now,
            UpdatedAt = now,
        };

        decimal subtotal = 0;
        foreach (var (productId, quantity) in lines)
        {
            if (!products.TryGetValue(productId, out var product) || !product.IsActive)
                return (null, CheckoutFailed("Bir ürün artık satışta değil veya bulunamadı."));

            if (product.UnitInStock < quantity)
                return (null, CheckoutFailed($"Yetersiz stok: {product.ProductName}. Miktarı azaltın veya sepetten çıkarın."));

            var unitPrice = ProductPricing.EffectiveUnitPrice(product.UnitPrice, product.Discount);
            order.OrderItems.Add(new OrderItem
            {
                ProductId = productId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                Discount = 0,
                CreatedAt = now,
                UpdatedAt = now,
            });
            subtotal += unitPrice * quantity;
        }

        var (_, _, grandTotal) = ShippingQuote.Calculate(subtotal, _checkoutOptions);
        var payment = await _paymentSimulator.AuthorizeAsync(paymentMethod, grandTotal);
        if (!payment.Approved)
            return (null, CheckoutFailed(payment.MessageTr));

        // Stok düşümü; Product.UnitInStock eşzamanlılık belirteci olduğundan çakışma 409 CONFLICT olur.
        foreach (var (productId, quantity) in lines)
        {
            products[productId].UnitInStock -= quantity;
            products[productId].UpdatedAt = now;
        }

        order.TotalAmount = grandTotal;
        _context.Orders.Add(order);
        _context.CartItems.RemoveRange(await _context.CartItems.Where(c => c.UserId == userId).ToListAsync());
        await _context.SaveChangesAsync();

        _context.Notifications.Add(OrderNotifications.Placed(order));
        await _integrationEvents.PublishAsync(new OrderPlacedIntegrationEvent(
            order.Id,
            order.OrderNumber,
            userId,
            order.TotalAmount,
            order.Status,
            DateTimeOffset.UtcNow));
        await _context.SaveChangesAsync();

        await transaction.CommitAsync();
        return (order, null);
    }

    private async Task<BaseResponseDto<OrderDto>?> ValidateAgainstCartAsync(
        int userId,
        IReadOnlyList<(int ProductId, int Quantity)> lines)
    {
        var cart = (await _cartService.GetCartAsync(userId)).Data!;
        var cartLines = cart.Items.Where(i => i.Quantity > 0).ToList();
        if (cartLines.Count == 0)
            return null;

        if (cartLines.Any(i => !i.IsAvailable))
        {
            return BaseResponseDto<OrderDto>.Fail(
                "Sepetinizde stokta olmayan veya miktarı aşan ürün var. Sepeti güncelleyip tekrar deneyin.",
                ErrorCodes.CartUnavailable);
        }

        var fromCart = cartLines
            .OrderBy(i => i.ProductId)
            .Select(i => (i.ProductId, i.Quantity))
            .ToList();
        if (!lines.SequenceEqual(fromCart))
        {
            return BaseResponseDto<OrderDto>.Fail(
                "Sepet ile ödeme özeti uyuşmuyor. Sayfayı yenileyip tekrar deneyin.",
                ErrorCodes.CartMismatch);
        }

        return null;
    }

    /// <summary>Yönetici durum güncellemesi (§5.4).</summary>
    public async Task<BaseResponseDto<OrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto)
    {
        if (!OrderStatuses.TryNormalize(updateOrderStatusDto.Status, out var next))
        {
            return BaseResponseDto<OrderDto>.Fail(
                $"Geçersiz durum. Kullanın: {string.Join(", ", OrderStatuses.All)}.",
                ErrorCodes.InvalidStatus);
        }

        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        if (order == null)
            return OrderNotFound<OrderDto>();

        if (!string.IsNullOrWhiteSpace(updateOrderStatusDto.Notes))
            order.Notes = updateOrderStatusDto.Notes.Trim();
        if (next == OrderStatuses.ReturnRequested)
            order.ReturnRequestedAt ??= DateTime.UtcNow;

        await ChangeStatusAsync(order, next);
        await _context.SaveChangesAsync();

        return BaseResponseDto<OrderDto>.SuccessResult("Sipariş durumu güncellendi", ToDto(order));
    }

    /// <summary>Müşteri iptali (§5.3): yalnızca Pending/Processing; stok iade edilir.</summary>
    public async Task<BaseResponseDto<string>> CancelOrderAsync(int orderId, int userId, string? reason)
    {
        var order = await FindOwnedOrderAsync(orderId, userId);
        if (order == null)
            return OrderNotFound<string>();

        if (!OrderStatuses.IsCustomerCancellable(CanonicalStatus(order)))
            return BaseResponseDto<string>.Fail("Bu sipariş iptal edilemez.", ErrorCodes.CancelNotAllowed);

        order.CancelReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        await ChangeStatusAsync(order, OrderStatuses.Cancelled);
        await _context.SaveChangesAsync();

        return BaseResponseDto<string>.SuccessResult("Sipariş iptal edildi", "Sipariş iptal edildi");
    }

    /// <summary>İade talebi (§5.3): yalnızca Delivered → ReturnRequested.</summary>
    public async Task<BaseResponseDto<OrderDto>> RequestReturnAsync(int orderId, int userId, string reason)
    {
        var order = await FindOwnedOrderAsync(orderId, userId);
        if (order == null)
            return OrderNotFound<OrderDto>();

        if (CanonicalStatus(order) != OrderStatuses.Delivered)
        {
            return BaseResponseDto<OrderDto>.Fail(
                "Yalnızca teslim edilmiş siparişler için iade talebi oluşturulabilir.",
                ErrorCodes.ReturnNotAllowed);
        }

        order.ReturnReason = reason.Trim();
        order.ReturnRequestedAt = DateTime.UtcNow;
        await ChangeStatusAsync(order, OrderStatuses.ReturnRequested);
        await _context.SaveChangesAsync();

        return BaseResponseDto<OrderDto>.SuccessResult("İade talebiniz alındı", ToDto(order));
    }

    /// <summary>Demo lojistik (§5.5): Pending → Processing → Shipped → Delivered.</summary>
    public async Task<BaseResponseDto<OrderDto>> AdvanceDemoFulfillmentAsync(int orderId, int userId)
    {
        if (!_checkoutOptions.DemoFulfillmentEnabled)
            return BaseResponseDto<OrderDto>.NotFound("Demo lojistik bu ortamda kapalı.", ErrorCodes.DemoFulfillmentDisabled);

        var order = await FindOwnedOrderAsync(orderId, userId);
        if (order == null)
            return OrderNotFound<OrderDto>();

        var next = OrderStatuses.NextDemoStep(CanonicalStatus(order));
        if (next == null)
        {
            return BaseResponseDto<OrderDto>.Fail(
                "Bu sipariş için ilerletilecek demo adımı yok.",
                ErrorCodes.DemoAdvanceInvalidState);
        }

        await ChangeStatusAsync(order, next);
        await _context.SaveChangesAsync();

        return BaseResponseDto<OrderDto>.SuccessResult("Sipariş durumu güncellendi", ToDto(order));
    }

    public async Task<BaseResponseDto<List<OrderDto>>> GetAllOrdersAsync(int pageNumber, int pageSize)
    {
        var paging = Paging.Normalize(pageNumber, pageSize <= 0 ? DefaultAdminPageSize : pageSize);
        var orders = await _orderRepository.GetAllOrdersAsync(paging.Skip, paging.PageSize);
        return BaseResponseDto<List<OrderDto>>.SuccessResult("Siparişler listelendi", orders.Select(ToDto).ToList());
    }

    /// <summary>
    /// Durum geçişi: stok iadesi, kargo/teslim alanları, kullanıcı bildirimi ve entegrasyon olayı. Kaydetmeyi çağıran yapar.
    /// </summary>
    private async Task ChangeStatusAsync(Order order, string next)
    {
        var previous = CanonicalStatus(order);
        if (previous == next)
            return;

        if (OrderStatuses.RestoresStock(previous, next))
        {
            foreach (var line in order.OrderItems)
            {
                line.Product.UnitInStock += line.Quantity;
                line.Product.UpdatedAt = DateTime.UtcNow;
            }
        }

        var now = DateTime.UtcNow;
        order.Status = next;
        order.UpdatedAt = now;

        if (next == OrderStatuses.Shipped)
        {
            order.ShippedAt = now;
            if (string.IsNullOrWhiteSpace(order.Carrier))
                order.Carrier = DefaultCarrier;
            if (string.IsNullOrWhiteSpace(order.TrackingNumber))
                order.TrackingNumber = GenerateTrackingNumber();
            order.EstimatedDeliveryAt = now.AddDays(3);
        }
        else if (next == OrderStatuses.Delivered)
        {
            order.DeliveredAt = now;
        }

        if (OrderNotifications.StatusChanged(order) is { } notification)
            _context.Notifications.Add(notification);

        await _integrationEvents.PublishAsync(new OrderStatusChangedIntegrationEvent(
            order.Id,
            order.OrderNumber,
            order.UserId,
            previous,
            next,
            DateTimeOffset.UtcNow));
    }

    /// <summary>Başkasının siparişi "bulunamadı" sayılır (404).</summary>
    private async Task<Order?> FindOwnedOrderAsync(int orderId, int userId)
    {
        var order = await _orderRepository.GetOrderByIdAsync(orderId);
        return order?.UserId == userId ? order : null;
    }

    private static string CanonicalStatus(Order order) =>
        OrderStatuses.TryNormalize(order.Status, out var canonical) ? canonical : order.Status;

    private BaseResponseDto<OrderDto> IdempotentReplay(Order order)
    {
        var replay = BaseResponseDto<OrderDto>.SuccessResult("Idempotent replay — same order as first request", ToDto(order));
        replay.StatusCode = 200;
        return replay;
    }

    private static BaseResponseDto<T> OrderNotFound<T>() =>
        BaseResponseDto<T>.NotFound("Sipariş bulunamadı", ErrorCodes.OrderNotFound);

    private static BaseResponseDto<OrderDto> CheckoutFailed(string message) =>
        BaseResponseDto<OrderDto>.Fail(message, ErrorCodes.CheckoutFailed);

    private OrderDto ToDto(Order order)
    {
        var items = order.OrderItems
            .OrderBy(i => i.Id)
            .Select(item => new OrderItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.ProductName ?? string.Empty,
                ProductImageUrl = item.Product?.ImageUrl,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice,
            })
            .ToList();

        var subtotal = Math.Round(items.Sum(i => i.TotalPrice), 2);
        var status = CanonicalStatus(order);

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            UserName = order.User != null ? $"{order.User.FirstName} {order.User.LastName}".Trim() : string.Empty,
            UserEmail = order.User?.Email ?? string.Empty,
            Items = items,
            SubtotalAmount = subtotal,
            ShippingFee = Math.Round(Math.Max(0, order.TotalAmount - subtotal), 2),
            TotalAmount = order.TotalAmount,
            Status = status,
            Notes = order.Notes,
            ShippingAddress = order.ShippingAddress is { } shipping ? AddressService.ToDto(shipping) : null,
            BillingAddress = order.BillingAddress is { } billing ? AddressService.ToDto(billing) : null,
            PaymentMethod = order.PaymentMethod is { } payment ? PaymentMethodService.ToDto(payment) : null,
            TrackingNumber = order.TrackingNumber,
            Carrier = order.Carrier,
            ShippedAt = order.ShippedAt,
            DeliveredAt = order.DeliveredAt,
            EstimatedDeliveryAt = order.EstimatedDeliveryAt,
            CancelReason = order.CancelReason,
            ReturnReason = order.ReturnReason,
            ReturnRequestedAt = order.ReturnRequestedAt,
            DemoNextAction = _checkoutOptions.DemoFulfillmentEnabled && OrderStatuses.NextDemoStep(status) != null
                ? DemoNextAction
                : null,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
        };
    }

    /// <summary><c>ORD-yyyyMMdd-XXXXXXXX</c> (8 büyük harf hex).</summary>
    private static string GenerateOrderNumber() =>
        $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    /// <summary><c>TR</c> + 10 hane.</summary>
    private static string GenerateTrackingNumber() =>
        "TR" + Random.Shared.NextInt64(0, 10_000_000_000).ToString("D10", System.Globalization.CultureInfo.InvariantCulture);
}
