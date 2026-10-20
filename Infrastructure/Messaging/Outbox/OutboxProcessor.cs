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

    /// <summary>Bu kadar başarısız denemeden sonra mesaj atlanır (dead-letter); <c>LastError</c> nedeni tutar.</summary>
    public const int MaxAttempts = 10;

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

        // ponytail: satır kilidi (FOR UPDATE SKIP LOCKED) yok; tek API instance'ı varsayılır. Ölçeklenirse
        // birden çok instance aynı mesajı yayınlayabilir — o zaman kilit ekleyin (consumer'lar zaten idempotent olmalı).
        var pending = await db.OutboxMessages
            .Where(m => m.IsActive && m.ProcessedAtUtc == null && m.Attempts < MaxAttempts)
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
                {
                    // Tekrar denemek düzeltmez: hemen dead-letter (Attempts = MaxAttempts) yap.
                    msg.Attempts = MaxAttempts;
                    msg.LastError = $"Unknown outbox message type: '{msg.Type}'";
                    msg.UpdatedAt = now;
                    logger.LogError("Outbox message {OutboxId} has unknown type {Type}; dead-lettered", msg.Id, msg.Type);
                    continue;
                }

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

        // Yalnızca beyaz listedeki tipler: DB'deki string'den rastgele tip yüklenmez.
        if (!KnownTypes.TryGetValue(typeKey, out var type))
            return false;

        var obj = JsonSerializer.Deserialize(payload, type, JsonOptions);
        if (obj == null)
            return false;

        integrationEvent = obj;
        return true;
    }
}

