using System.Globalization;
using EcommerceBackend.Domain.Entities;

namespace EcommerceBackend.Application.Orders;

/// <summary>
/// Sipariş sistem bildirimleri (§4.12; Type <c>Order</c>, actionUrl <c>/orders/{id}</c>). Sipariş değişikliğiyle aynı
/// transaction içinde yazılır; böylece mesaj kuyruğu olmadan da (geliştirme, InMemory) her zaman oluşur.
/// </summary>
public static class OrderNotifications
{
    public const string Type = "Order";

    public static Notification Placed(Order order) => Build(
        order,
        "Sipariş alındı",
        $"#{order.OrderNumber} siparişiniz oluşturuldu. Tutar: {order.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)}");

    /// <summary>Siparişin yeni durumu için bildirim; eşlenmeyen durumlarda (ör. Pending) null.</summary>
    public static Notification? StatusChanged(Order order) => order.Status switch
    {
        OrderStatuses.Processing => Build(order, "Sipariş hazırlanıyor", $"#{order.OrderNumber} siparişiniz hazırlanıyor."),
        OrderStatuses.Shipped => Build(order, "Sipariş kargoya verildi", $"#{order.OrderNumber} siparişiniz kargoya verildi."),
        OrderStatuses.Delivered => Build(order, "Sipariş teslim edildi", $"#{order.OrderNumber} siparişiniz teslim edildi."),
        OrderStatuses.Cancelled => Build(order, "Sipariş iptal edildi", $"#{order.OrderNumber} siparişiniz iptal edildi."),
        OrderStatuses.ReturnRequested => Build(order, "İade talebi alındı", $"#{order.OrderNumber} siparişiniz için iade talebi alındı."),
        OrderStatuses.Returned => Build(order, "İade tamamlandı", $"#{order.OrderNumber} siparişiniz için iade tamamlandı."),
        _ => null,
    };

    private static Notification Build(Order order, string title, string message) => new()
    {
        UserId = order.UserId,
        Title = title,
        Message = message,
        Type = Type,
        ActionUrl = $"/orders/{order.Id}",
        IsRead = false,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow,
    };
}
