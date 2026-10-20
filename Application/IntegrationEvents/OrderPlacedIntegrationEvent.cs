namespace EcommerceBackend.Application.IntegrationEvents;

/// <summary>
/// Sipariş oluşturulduğunda kuyruğa düşen olay (bildirim, e-posta, stok senkronu vb. tüketiciler için).
/// </summary>
public record OrderPlacedIntegrationEvent(
    int OrderId,
    string OrderNumber,
    int UserId,
    decimal TotalAmount,
    string Status,
    DateTimeOffset OccurredAt);
