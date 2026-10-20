using System.Text.Json;
using EcommerceBackend.Application.Abstractions.Messaging;
using EcommerceBackend.Domain.Entities;
using EcommerceBackend.Infrastructure.Data;

namespace EcommerceBackend.Infrastructure.Messaging.Outbox;

/// <summary>
/// Transactional outbox publisher: publish yerine OutboxMessages tablosuna yazar.
/// Arka plandaki OutboxProcessor bunu gerçek transport'a publish eder.
/// </summary>
public sealed class OutboxIntegrationEventPublisher(ApplicationDbContext db) : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class
    {
        var msg = new OutboxMessage
        {
            Type = integrationEvent.GetType().AssemblyQualifiedName ?? integrationEvent.GetType().FullName ?? integrationEvent.GetType().Name,
            Payload = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), JsonOptions),
            OccurredAtUtc = DateTime.UtcNow,
            ProcessedAtUtc = null,
            Attempts = 0,
            LastError = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.OutboxMessages.Add(msg);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }
}

