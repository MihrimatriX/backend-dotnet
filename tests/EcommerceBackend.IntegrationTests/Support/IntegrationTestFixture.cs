using Npgsql;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;
using Xunit;

namespace EcommerceBackend.IntegrationTests.Support;

/// <summary>
/// Docker üzerinde gerçek PostgreSQL, Redis ve RabbitMQ kapsayıcıları; tek fabrika tüm entegrasyon testleri için paylaşılır.
/// </summary>
public sealed class IntegrationTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:15-alpine")
        .WithDatabase("ecommerce_integration")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder("redis:7-alpine")
        .Build();

    private readonly RabbitMqContainer _rabbit = new RabbitMqBuilder("rabbitmq:3-management-alpine")
        .WithUsername("ecommerce")
        .WithPassword("ecommerce_test")
        .Build();

    public EcommerceWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await Task.WhenAll(
            _postgres.StartAsync(),
            _redis.StartAsync(),
            _rabbit.StartAsync());

        var redisPort = _redis.GetMappedPublicPort(6379);
        var rabbitPort = (ushort)_rabbit.GetMappedPublicPort(5672);

        // Test süreci Docker içindeyken (ör. sdk:10.0 imajı) yayınlanan portlara host üzerinden erişilir.
        var reachHost = Environment.GetEnvironmentVariable("TESTCONTAINERS_HOST_OVERRIDE");
        if (string.IsNullOrWhiteSpace(reachHost) && File.Exists("/.dockerenv"))
            reachHost = "host.docker.internal";

        var pgBuilder = new NpgsqlConnectionStringBuilder(_postgres.GetConnectionString());
        if (!string.IsNullOrWhiteSpace(reachHost))
        {
            pgBuilder.Host = reachHost;
            pgBuilder.Port = _postgres.GetMappedPublicPort(5432);
        }

        var redisHostname = string.IsNullOrWhiteSpace(reachHost) ? _redis.Hostname : reachHost;
        var rabbitHostname = string.IsNullOrWhiteSpace(reachHost) ? _rabbit.Hostname : reachHost;

        // StackExchange.Redis ilk bağlantıda bekleme; health check zaman aşımına düşmesin
        var redisConnection = $"{redisHostname}:{redisPort},abortConnect=false,connectTimeout=10000";

        Factory = new EcommerceWebApplicationFactory(
            pgBuilder.ConnectionString,
            redisConnection,
            rabbitHostname,
            rabbitPort);
    }

    public async Task DisposeAsync()
    {
        if (Factory != null)
            await Factory.DisposeAsync();

        await _rabbit.DisposeAsync();
        await _redis.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}

[CollectionDefinition(nameof(IntegrationCollection))]
public class IntegrationCollection : ICollectionFixture<IntegrationTestFixture>
{
}
