using EcommerceBackend.Application.Abstractions.Messaging;
using EcommerceBackend.Application.DTOs;
using EcommerceBackend.Application.IntegrationEvents;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using EcommerceBackend.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
namespace EcommerceBackend.Application.Services;

public class OrderService : IOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IPaymentMethodRepository _paymentMethodRepository;
    private readonly IIntegrationEventPublisher _integrationEvents;
    private readonly ICartService _cartService;
    private readonly ICheckoutPaymentSimulator _paymentSimulator;
    private readonly CheckoutOptions _checkoutOptions;

    public OrderService(
        ApplicationDbContext context,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IAddressRepository addressRepository,
        IPaymentMethodRepository paymentMethodRepository,
        IIntegrationEventPublisher integrationEvents,
        ICartService cartService,
        ICheckoutPaymentSimulator paymentSimulator,
        IOptions<CheckoutOptions> checkoutOptions)
    {
        _context = context;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _addressRepository = addressRepository;
        _paymentMethodRepository = paymentMethodRepository;
        _integrationEvents = integrationEvents;
        _cartService = cartService;
        _paymentSimulator = paymentSimulator;
        _checkoutOptions = checkoutOptions.Value;
    }

    public async Task<BaseResponseDto<List<OrderDto>>> GetUserOrdersAsync(int userId)
    {
        try
        {
            var orders = await _orderRepository.GetUserOrdersAsync(userId);
            var orderDtos = orders.Select(ConvertToDto).ToList();
            return BaseResponseDto<List<OrderDto>>.SuccessResult("Siparişler listelendi", orderDtos);
        }
        catch (Exception ex)
        {
            return BaseResponseDto<List<OrderDto>>.ErrorResult($"Siparişler alınamadı: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto<OrderDto>> GetOrderByIdAsync(int orderId, int userId)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != userId)
                return BaseResponseDto<OrderDto>.ErrorResultWithCode("Sipariş bulunamadı", "ORDER_NOT_FOUND");

            return BaseResponseDto<OrderDto>.SuccessResult("Sipariş getirildi", ConvertToDto(order));
        }
        catch (Exception ex)
        {
            return BaseResponseDto<OrderDto>.ErrorResult($"Sipariş alınamadı: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto<OrderDto>> CreateOrderAsync(int userId, CreateOrderDto createOrderDto)
    {
        try
        {
            return await CreateOrderCoreAsync(userId, createOrderDto);
        }
        catch (Exception ex)
        {
            return BaseResponseDto<OrderDto>.ErrorResult($"Sipariş oluşturulamadı: {ex.Message}");
        }
    }

    private async Task<BaseResponseDto<OrderDto>> CreateOrderCoreAsync(int userId, CreateOrderDto createOrderDto)
    {
        var grouped = createOrderDto.Items
            .GroupBy(i => i.ProductId)
            .Select(g => (ProductId: g.Key, Quantity: g.Sum(x => x.Quantity)))
            .ToList();

        if (grouped.Count == 0)
            return BaseResponseDto<OrderDto>.ErrorResultWithCode("Sepette ürün yok.", "EMPTY_ORDER");

        var shippingAddress = await _addressRepository.GetByIdAsync(createOrderDto.ShippingAddressId);
        if (shippingAddress == null || shippingAddress.UserId != userId)
            return BaseResponseDto<OrderDto>.ErrorResultWithCode("Teslimat adresi geçersiz veya size ait değil.", "INVALID_ADDRESS");

        var paymentMethod = await _paymentMethodRepository.GetByIdAsync(createOrderDto.PaymentMethodId);
        if (paymentMethod == null || paymentMethod.UserId != userId)
            return BaseResponseDto<OrderDto>.ErrorResultWithCode("Ödeme yöntemi geçersiz veya size ait değil.", "INVALID_PAYMENT");

        var cartCheck = await _cartService.GetCartAsync(userId);
        if (cartCheck is { Success: true, Data: { } cartData })
        {
            var cartLines = cartData.Items.Where(i => i.Quantity > 0).ToList();
            if (cartLines.Count > 0)
            {
                if (cartLines.Any(i => !i.IsAvailable))
                {
                    return BaseResponseDto<OrderDto>.ErrorResultWithCode(
                        "Sepetinizde stokta olmayan veya miktarı aşan ürün var. Sepeti güncelleyip tekrar deneyin.",
                        "CART_UNAVAILABLE");
                }

                var requested = grouped.OrderBy(i => i.ProductId).ToList();
                var fromCart = cartLines
                    .OrderBy(i => i.ProductId)
                    .Select(i => (i.ProductId, i.Quantity))
                    .ToList();
                if (requested.Count != fromCart.Count ||
                    requested.Zip(fromCart).Any(z =>
                        z.First.ProductId != z.Second.ProductId ||
                        z.First.Quantity != z.Second.Quantity))
                {
                    return BaseResponseDto<OrderDto>.ErrorResultWithCode(
                        "Sepet ile ödeme özeti uyuşmuyor. Sayfayı yenileyip tekrar deneyin.",
                        "CART_MISMATCH");
                }
            }
        }

        string? flowError = null;
        Order? created = null;

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var orderItems = new List<OrderItem>();
                decimal subtotal = 0;
                var trackedProducts = new List<(Product Product, int Qty)>();

                foreach (var g in grouped)
                {
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == g.ProductId);
                    if (product == null || !product.IsActive)
                    {
                        flowError = "Bir ürün artık satışta değil veya bulunamadı.";
                        await tx.RollbackAsync();
                        return;
                    }

                    if (product.UnitInStock < g.Quantity)
                    {
                        flowError = $"Yetersiz stok: {product.ProductName}. Miktarı azaltın veya sepetten çıkarın.";
                        await tx.RollbackAsync();
                        return;
                    }

                    var unitCharged = ProductPricing.EffectiveUnitPrice(product.UnitPrice, product.Discount);
                    orderItems.Add(new OrderItem
                    {
                        ProductId = g.ProductId,
                        Quantity = g.Quantity,
                        UnitPrice = unitCharged,
                        Discount = 0,
                    });
                    subtotal += unitCharged * g.Quantity;
                    trackedProducts.Add((product, g.Quantity));
                }

                var (_, shippingFee, grandTotal) = ShippingQuote.Calculate(subtotal, _checkoutOptions);

                var pay = await _paymentSimulator.AuthorizeAsync(userId, createOrderDto.PaymentMethodId, grandTotal);
                if (!pay.Approved)
                {
                    flowError = pay.MessageTr;
                    await tx.RollbackAsync();
                    return;
                }

                foreach (var (p, qty) in trackedProducts)
                {
                    p.UnitInStock -= qty;
                    p.UpdatedAt = DateTime.UtcNow;
                }

                var order = new Order
                {
                    OrderNumber = GenerateOrderNumber(),
                    UserId = userId,
                    TotalAmount = grandTotal,
                    Status = "Pending",
                    ShippingAddressId = createOrderDto.ShippingAddressId,
                    BillingAddressId = createOrderDto.ShippingAddressId,
                    PaymentMethodId = createOrderDto.PaymentMethodId,
                    Notes = createOrderDto.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                foreach (var oi in orderItems)
                    order.OrderItems.Add(oi);

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                await _cartService.ClearCartAsync(userId);
                await _context.SaveChangesAsync();

                await _integrationEvents.PublishAsync(
                    new OrderPlacedIntegrationEvent(
                        order.Id,
                        order.OrderNumber,
                        userId,
                        order.TotalAmount,
                        order.Status,
                        DateTimeOffset.UtcNow));

                await tx.CommitAsync();
                created = order;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });

        if (flowError != null)
            return BaseResponseDto<OrderDto>.ErrorResultWithCode(flowError, "CHECKOUT_FAILED");

        if (created == null)
            return BaseResponseDto<OrderDto>.ErrorResultWithCode("Sipariş tamamlanamadı.", "CHECKOUT_FAILED");

        var reloaded = await _orderRepository.GetOrderByIdAsync(created.Id);
        if (reloaded == null)
            return BaseResponseDto<OrderDto>.ErrorResult("Sipariş kaydedildi ancak yüklenemedi.");

        return BaseResponseDto<OrderDto>.SuccessResult("Siparişiniz alındı", ConvertToDto(reloaded));
    }


    public async Task<BaseResponseDto<OrderDto>> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusDto updateOrderStatusDto)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                return BaseResponseDto<OrderDto>.ErrorResultWithCode("Sipariş bulunamadı", "ORDER_NOT_FOUND");

            var allowed = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled", };
            if (!allowed.Contains(updateOrderStatusDto.Status, StringComparer.OrdinalIgnoreCase))
            {
                return BaseResponseDto<OrderDto>.ErrorResult(
                    "Geçersiz durum. Kullanın: Pending, Processing, Shipped, Delivered veya Cancelled.");
            }

            var prev = order.Status;
            var next = updateOrderStatusDto.Status;

            if (string.Equals(next, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(prev, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(prev, "Pending", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(prev, "Processing", StringComparison.OrdinalIgnoreCase)))
            {
                await RestoreStockForOrderLinesAsync(order.OrderItems);
            }

            order.Status = next;
            order.UpdatedAt = DateTime.UtcNow;
            if (string.Equals(next, "Shipped", StringComparison.OrdinalIgnoreCase) && order.ShippedAt == null)
                order.ShippedAt = DateTime.UtcNow;
            if (string.Equals(next, "Delivered", StringComparison.OrdinalIgnoreCase) && order.DeliveredAt == null)
                order.DeliveredAt = DateTime.UtcNow;

            var updatedOrder = await _orderRepository.UpdateAsync(order);

            if (!string.Equals(prev, next, StringComparison.OrdinalIgnoreCase))
            {
                await _integrationEvents.PublishAsync(
                    new OrderStatusChangedIntegrationEvent(
                        updatedOrder.Id,
                        updatedOrder.OrderNumber,
                        updatedOrder.UserId,
                        prev,
                        next,
                        DateTimeOffset.UtcNow));
            }

            return BaseResponseDto<OrderDto>.SuccessResult("Sipariş durumu güncellendi", ConvertToDto(updatedOrder));
        }
        catch (Exception ex)
        {
            return BaseResponseDto<OrderDto>.ErrorResult($"Durum güncellenemedi: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto<string>> CancelOrderAsync(int orderId, int userId)
    {
        try
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null || order.UserId != userId)
                return BaseResponseDto<string>.ErrorResultWithCode("Sipariş bulunamadı", "ORDER_NOT_FOUND");

            if (order.Status == "Delivered" || order.Status == "Cancelled")
                return BaseResponseDto<string>.ErrorResultWithCode("Bu sipariş iptal edilemez.", "CANCEL_NOT_ALLOWED");

            var prev = order.Status;
            await RestoreStockForOrderLinesAsync(order.OrderItems);

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            await _integrationEvents.PublishAsync(
                new OrderStatusChangedIntegrationEvent(
                    order.Id,
                    order.OrderNumber,
                    order.UserId,
                    prev,
                    order.Status,
                    DateTimeOffset.UtcNow));

            return BaseResponseDto<string>.SuccessResult("Sipariş iptal edildi", "Sipariş iptal edildi");
        }
        catch (Exception ex)
        {
            return BaseResponseDto<string>.ErrorResult($"İptal başarısız: {ex.Message}");
        }
    }

    public async Task<BaseResponseDto<List<OrderDto>>> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var orders = await _orderRepository.GetAllOrdersAsync();
            var orderDtos = orders.Select(ConvertToDto).ToList();
            return BaseResponseDto<List<OrderDto>>.SuccessResult("Siparişler listelendi", orderDtos);
        }
        catch (Exception ex)
        {
            return BaseResponseDto<List<OrderDto>>.ErrorResult($"Siparişler alınamadı: {ex.Message}");
        }
    }

    private async Task RestoreStockForOrderLinesAsync(IEnumerable<OrderItem> lines)
    {
        foreach (var line in lines)
        {
            var product = await _productRepository.GetByIdAsync(line.ProductId);
            if (product == null)
                continue;
            product.UnitInStock += line.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
            await _productRepository.UpdateAsync(product.Id, product);
        }
    }

    private OrderDto ConvertToDto(Order order)
    {
        var items = order.OrderItems?.Select(item => new OrderItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product?.ProductName ?? "",
            ProductImageUrl = item.Product?.ImageUrl,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice,
        }).ToList() ?? new List<OrderItemDto>();

        var subtotal = items.Sum(i => i.TotalPrice);
        var shipping = Math.Max(0, order.TotalAmount - subtotal);

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            UserId = order.UserId,
            UserName = order.User != null ? $"{order.User.FirstName} {order.User.LastName}".Trim() : "",
            UserEmail = order.User?.Email ?? "",
            Items = items,
            SubtotalAmount = Math.Round(subtotal, 2),
            ShippingFee = Math.Round(shipping, 2),
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress != null
                ? new AddressDto
                {
                    Id = order.ShippingAddress.Id,
                    Title = order.ShippingAddress.Title,
                    FullAddress = order.ShippingAddress.FullAddress,
                    City = order.ShippingAddress.City,
                    District = order.ShippingAddress.District,
                    PostalCode = order.ShippingAddress.PostalCode,
                    PhoneNumber = order.ShippingAddress.PhoneNumber,
                    IsDefault = order.ShippingAddress.IsDefault,
                }
                : new AddressDto(),
            BillingAddress = order.BillingAddress != null
                ? new AddressDto
                {
                    Id = order.BillingAddress.Id,
                    Title = order.BillingAddress.Title,
                    FullAddress = order.BillingAddress.FullAddress,
                    City = order.BillingAddress.City,
                    District = order.BillingAddress.District,
                    PostalCode = order.BillingAddress.PostalCode,
                    PhoneNumber = order.BillingAddress.PhoneNumber,
                    IsDefault = order.BillingAddress.IsDefault,
                }
                : new AddressDto(),
            PaymentMethod = order.PaymentMethod != null
                ? new PaymentMethodDto
                {
                    Id = order.PaymentMethod.Id,
                    Type = order.PaymentMethod.Type,
                    CardNumber = order.PaymentMethod.CardNumber,
                    CardHolderName = order.PaymentMethod.CardHolderName,
                    ExpiryMonth = order.PaymentMethod.ExpiryMonth,
                    ExpiryYear = order.PaymentMethod.ExpiryYear,
                    IsDefault = order.PaymentMethod.IsDefault,
                    BankName = order.PaymentMethod.BankName,
                    AccountNumber = order.PaymentMethod.AccountNumber,
                    AccountHolderName = order.PaymentMethod.AccountHolderName,
                }
                : null,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
        };
    }

    private static string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
    }
}
