using EcommerceBackend.Application.Abstractions.Caching;
using EcommerceBackend.Application.Abstractions.Messaging;
using EcommerceBackend.Application.Options;
using EcommerceBackend.Infrastructure.Caching;
using EcommerceBackend.Infrastructure.Messaging;
using EcommerceBackend.Infrastructure.Messaging.Consumers;
using EcommerceBackend.Infrastructure.Messaging.Outbox;
using EcommerceBackend.Infrastructure.Options;
using System.Net.Sockets;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EcommerceBackend.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static string ResolveRedisConnectionString(this IConfiguration configuration) =>
        configuration["Redis:ConnectionString"]
        ?? configuration.GetConnectionString("Redis")
        ?? "localhost:6379";

    /// <summary>
    /// Redis dağıtık önbellek, katalog önbelleği ve MassTransit (RabbitMQ veya InMemory).
    /// </summary>
    public static IServiceCollection AddEcommerceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisInfrastructureOptions>(configuration.GetSection(RedisInfrastructureOptions.SectionName));
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));
        services.Configure<CatalogCacheOptions>(configuration.GetSection(CatalogCacheOptions.SectionName));

        var redisConnection = configuration.ResolveRedisConnectionString();
        var redisSection = configuration.GetSection(RedisInfrastructureOptions.SectionName)
            .Get<RedisInfrastructureOptions>() ?? new RedisInfrastructureOptions();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = redisSection.InstanceName;
        });

        services.AddSingleton<ICatalogReadCache, RedisCatalogReadCache>();
        services.AddScoped<IIntegrationEventPublisher, OutboxIntegrationEventPublisher>();
        services.AddScoped<OutboxProcessor>();
        services.AddHostedService<OutboxProcessorHostedService>();

        var rabbit = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        var transport = rabbit.Transport.Trim();

        services.AddMassTransit(busConfiguration =>
        {
            busConfiguration.AddConsumer<OrderPlacedConsumer>();
            busConfiguration.AddConsumer<OrderStatusChangedConsumer>();

            if (string.Equals(transport, "RabbitMq", StringComparison.OrdinalIgnoreCase))
            {
                busConfiguration.UsingRabbitMq((context, cfg) =>
                {
                    var amqpPort = rabbit.Port == 0 ? (ushort)5672 : rabbit.Port;
                    cfg.Host(rabbit.Host, amqpPort, rabbit.VirtualHost, h =>
                    {
                        h.Username(rabbit.Username);
                        h.Password(rabbit.Password);
                    });

                    // Transport düzeyinde UseMessageRetry, host dururken gecikmeli yeniden denemelerle elenmiş IServiceProvider'a
                    // scope açmaya çalışıp ObjectDisposedException logları üretebiliyor (özellikle test/WebApplicationFactory kapanışı).
                    // Dayanıklılık gerekiyorsa RabbitMQ tarafında DLX/redelivery veya ayrı outbox/retry stratejisi tercih edilir.

                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                busConfiguration.UsingInMemory((context, cfg) => { cfg.ConfigureEndpoints(context); });
            }
        });

        return services;
    }

    public static IHealthChecksBuilder AddEcommerceInfrastructureHealthChecks(
        this IHealthChecksBuilder healthChecks,
        IConfiguration configuration,
        string redisConnectionString)
    {
        healthChecks.AddRedis(redisConnectionString, name: "redis", tags: new[] { "infra", "cache", "ready" });

        var rabbit = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>() ?? new RabbitMqOptions();
        if (!string.Equals(rabbit.Transport, "RabbitMq", StringComparison.OrdinalIgnoreCase))
            return healthChecks;

        // AspNetCore.HealthChecks.Rabbitmq, RabbitMQ.Client 6 API kullanır; MassTransit 8 ise Client 7 getirir (IModel uyumsuz).
        var amqpHost = rabbit.Host;
        var amqpPort = rabbit.Port == 0 ? 5672 : rabbit.Port;
        healthChecks.AddAsyncCheck(
            "rabbitmq",
            async (ct) =>
            {
                try
                {
                    using var tcp = new TcpClient();
                    await tcp.ConnectAsync(amqpHost, amqpPort, ct).ConfigureAwait(false);
                    return HealthCheckResult.Healthy();
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("AMQP port unreachable", ex);
                }
            },
            tags: new[] { "infra", "messaging", "ready" });

        return healthChecks;
    }
}
