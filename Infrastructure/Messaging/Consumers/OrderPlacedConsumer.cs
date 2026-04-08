using EcommerceBackend.Application.IntegrationEvents;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Messaging.Consumers;

/// <summary>
/// Sipariş olayını işler: örnek olarak uygulama içi bildirim kaydı (genişletilebilir: e-posta, webhook).
/// </summary>
public sealed class OrderPlacedConsumer : IConsumer<OrderPlacedIntegrationEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderPlacedConsumer> _logger;

    public OrderPlacedConsumer(IServiceScopeFactory scopeFactory, ILogger<OrderPlacedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
    {
        var msg = context.Message;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var userExists = await db.Users.AnyAsync(u => u.Id == msg.UserId && u.IsActive, context.CancellationToken)
            .ConfigureAwait(false);
        if (!userExists)
        {
            _logger.LogWarning("OrderPlaced event for missing user {UserId}, order {OrderId}", msg.UserId, msg.OrderId);
            return;
        }

        var amountText = msg.TotalAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);
        var notification = new Notification
        {
            UserId = msg.UserId,
            Title = "Sipariş alındı",
            Message = $"#{msg.OrderNumber} siparişiniz oluşturuldu. Tutar: {amountText}",
            Type = "Order",
            ActionUrl = $"/orders/{msg.OrderId}",
            IsRead = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        db.Notifications.Add(notification);
        await db.SaveChangesAsync(context.CancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Persisted order-placed notification for user {UserId}, order {OrderNumber}",
            msg.UserId,
            msg.OrderNumber);
    }
}
