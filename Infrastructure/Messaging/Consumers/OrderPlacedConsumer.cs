using EcommerceBackend.Application.IntegrationEvents;
using MassTransit;

namespace EcommerceBackend.Infrastructure.Messaging.Consumers;

/// <summary>
/// Sipariş oluşturuldu olayı. Kullanıcıya giden "Sipariş alındı" bildirimi sipariş ile aynı transaction içinde
/// yazılır (OrderService); bu tüketici e-posta, webhook, ERP senkronu gibi dış entegrasyonlar için genişletme noktasıdır.
/// </summary>
public sealed class OrderPlacedConsumer : IConsumer<OrderPlacedIntegrationEvent>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        var msg = context.Message;
        _logger.LogInformation(
            "OrderPlaced: order {OrderNumber} ({OrderId}) for user {UserId}, total {TotalAmount}",
            msg.OrderNumber,
            msg.OrderId,
            msg.UserId,
            msg.TotalAmount);
        return Task.CompletedTask;
    }
}
