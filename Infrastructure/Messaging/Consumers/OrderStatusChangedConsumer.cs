using EcommerceBackend.Application.IntegrationEvents;
using MassTransit;

namespace EcommerceBackend.Infrastructure.Messaging.Consumers;

/// <summary>
/// Sipariş durum değişimi olayı. Kullanıcı bildirimi durum değişikliğiyle aynı transaction içinde yazılır
/// (OrderService); bu tüketici dış entegrasyonlar (e-posta, webhook, CRM) için genişletme noktasıdır.
/// </summary>
public sealed class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedIntegrationEvent>
{
    private readonly ILogger<OrderStatusChangedConsumer> _logger;

    public OrderStatusChangedConsumer(ILogger<OrderStatusChangedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderStatusChangedIntegrationEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "OrderStatusChanged: order {OrderNumber} ({OrderId}) {Previous} -> {Current}",
            msg.OrderNumber,
            msg.OrderId,
            msg.PreviousStatus,
            msg.CurrentStatus);
        return Task.CompletedTask;
    }
}
