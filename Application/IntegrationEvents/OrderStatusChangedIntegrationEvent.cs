namespace EcommerceBackend.Application.IntegrationEvents;

/// <summary>
/// Sipariş durum geçişi olduğunda yayınlanan olay (bildirim, e-posta, webhook, ERP senkronu vb. tüketiciler için).
/// </summary>
public record OrderStatusChangedIntegrationEvent(
    int OrderId,
    string OrderNumber,
    int UserId,
    string PreviousStatus,
    string CurrentStatus,
    DateTimeOffset OccurredAt);

