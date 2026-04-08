using EcommerceBackend.Application.IntegrationEvents;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Messaging.Consumers;

/// <summary>
/// Sipariş durum değişimlerini işler: uygulama içi bildirim kaydı (genişletilebilir: e-posta, webhook, CRM senkronu).
/// </summary>
public sealed class OrderStatusChangedConsumer : IConsumer<OrderStatusChangedIntegrationEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OrderStatusChangedConsumer> _logger;

    public OrderStatusChangedConsumer(IServiceScopeFactory scopeFactory, ILogger<OrderStatusChangedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderStatusChangedIntegrationEvent> context)
    {
        var msg = context.Message;

        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var userExists = await db.Users.AnyAsync(u => u.Id == msg.UserId && u.IsActive, context.CancellationToken)
            .ConfigureAwait(false);
        if (!userExists)
        {
            _logger.LogWarning(
                "OrderStatusChanged event for missing user {UserId}, order {OrderId}",
                msg.UserId,
                msg.OrderId);
            return;
        }

        var (title, message) = BuildNotificationText(msg.OrderNumber, msg.PreviousStatus, msg.CurrentStatus);
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(message))
        {
            _logger.LogInformation(
                "OrderStatusChanged ignored (no notification mapping). order {OrderNumber} {Prev}->{Next}",
                msg.OrderNumber,
                msg.PreviousStatus,
                msg.CurrentStatus);
            return;
        }

        var notification = new Notification
        {
            UserId = msg.UserId,
            Title = title,
            Message = message,
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
            "Persisted order-status notification for user {UserId}, order {OrderNumber}: {Prev}->{Next}",
            msg.UserId,
            msg.OrderNumber,
            msg.PreviousStatus,
            msg.CurrentStatus);
    }

    private static (string Title, string Message) BuildNotificationText(string orderNumber, string prev, string next)
    {
        if (string.Equals(prev, next, StringComparison.OrdinalIgnoreCase))
            return ("", "");

        if (string.Equals(next, "Processing", StringComparison.OrdinalIgnoreCase))
            return ("Sipariş hazırlanıyor", $"#{orderNumber} siparişiniz hazırlanıyor.");

        if (string.Equals(next, "Shipped", StringComparison.OrdinalIgnoreCase))
            return ("Sipariş kargoya verildi", $"#{orderNumber} siparişiniz kargoya verildi.");

        if (string.Equals(next, "Delivered", StringComparison.OrdinalIgnoreCase))
            return ("Sipariş teslim edildi", $"#{orderNumber} siparişiniz teslim edildi.");

        if (string.Equals(next, "Cancelled", StringComparison.OrdinalIgnoreCase))
            return ("Sipariş iptal edildi", $"#{orderNumber} siparişiniz iptal edildi.");

        return ("", "");
    }
}

