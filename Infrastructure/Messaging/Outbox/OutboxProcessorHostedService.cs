using Microsoft.Extensions.Hosting;

namespace EcommerceBackend.Infrastructure.Messaging.Outbox;

public sealed class OutboxProcessorHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessorHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Outbox processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<OutboxProcessor>();

                var processed = await processor.ProcessBatchAsync(batchSize: 50, stoppingToken).ConfigureAwait(false);
                await Task.Delay(processed > 0 ? TimeSpan.FromMilliseconds(250) : TimeSpan.FromSeconds(2), stoppingToken)
                    .ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // graceful shutdown
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processor loop error.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }

        logger.LogInformation("Outbox processor stopped.");
    }
}

