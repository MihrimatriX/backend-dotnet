using System.Text.Json;
using EcommerceBackend.Application.IntegrationEvents;
using EcommerceBackend.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace EcommerceBackend.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IPublishEndpoint publishEndpoint,
    ILogger<OutboxProcessor> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly IReadOnlyDictionary<string, Type> KnownTypes = new Dictionary<string, Type>(StringComparer.Ordinal)
    {
        // Stable keys: AssemblyQualifiedName ile yazıldığı için genelde birebir eşleşir; yine de FullName fallback var.
        { typeof(OrderPlacedIntegrationEvent).AssemblyQualifiedName!, typeof(OrderPlacedIntegrationEvent) },
        { typeof(OrderPlacedIntegrationEvent).FullName!, typeof(OrderPlacedIntegrationEvent) },
        { typeof(OrderStatusChangedIntegrationEvent).AssemblyQualifiedName!, typeof(OrderStatusChangedIntegrationEvent) },
        { typeof(OrderStatusChangedIntegrationEvent).FullName!, typeof(OrderStatusChangedIntegrationEvent) },
    };

    public async Task<int> ProcessBatchAsync(int batchSize, CancellationToken ct)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var now = DateTime.UtcNow;

        var pending = await db.OutboxMessages
            .Where(m => m.IsActive && m.ProcessedAtUtc == null)
            .OrderBy(m => m.OccurredAtUtc)
            .ThenBy(m => m.Id)
            .Take(batchSize)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var processed = 0;

        foreach (var msg in pending)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                if (!TryDeserialize(msg.Type, msg.Payload, out var integrationEvent))
                    throw new InvalidOperationException($"Unknown outbox message type: '{msg.Type}'");

                await publishEndpoint.Publish(integrationEvent, integrationEvent.GetType(), ct).ConfigureAwait(false);

                msg.ProcessedAtUtc = now;
                msg.Attempts += 1;
                msg.LastError = null;
                msg.UpdatedAt = now;
                processed += 1;
            }
            catch (Exception ex)
            {
                msg.Attempts += 1;
                msg.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                msg.UpdatedAt = now;

                logger.LogError(ex, "Outbox publish failed for {OutboxId} type {Type}", msg.Id, msg.Type);
            }
        }

        if (pending.Count > 0)
            await db.SaveChangesAsync(ct).ConfigureAwait(false);

        return processed;
    }

    private static bool TryDeserialize(string typeKey, string payload, out object integrationEvent)
    {
        integrationEvent = null!;

        if (!KnownTypes.TryGetValue(typeKey, out var type))
        {
            // AssemblyQualifiedName değiştiyse Type.GetType ile şansımızı deneyelim
            type = Type.GetType(typeKey, throwOnError: false);
        }

        if (type == null)
            return false;

        var obj = JsonSerializer.Deserialize(payload, type, JsonOptions);
        if (obj == null)
            return false;

        integrationEvent = obj;
        return true;
    }
}

