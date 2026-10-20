using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EcommerceBackend.IntegrationTests.Support;

/// <summary>
/// Testcontainers ile verilen gerçek Postgres, Redis ve RabbitMQ bağlantılarıyla API host'u üretir.
/// </summary>
public sealed class EcommerceWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _postgresConnectionString;
    private readonly string _redisConnectionString;
    private readonly string _rabbitHost;
    private readonly ushort _rabbitPort;

    public EcommerceWebApplicationFactory(
        string postgresConnectionString,
        string redisConnectionString,
        string rabbitHost,
        ushort rabbitPort)
    {
        _postgresConnectionString = postgresConnectionString;
        _redisConnectionString = redisConnectionString;
        _rabbitHost = rabbitHost;
        _rabbitPort = rabbitPort;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Integration");

        var sutContentRoot = Path.GetDirectoryName(typeof(Program).Assembly.Location)
            ?? AppContext.BaseDirectory;
        builder.UseContentRoot(sutContentRoot);

        // UseSetting, WebApplicationFactory + minimal Program.cs ile appsettings üzerine güvenilir şekilde yazar.
        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgresConnectionString);
        builder.UseSetting("Database:Provider", "Npgsql");
        builder.UseSetting("Redis:ConnectionString", _redisConnectionString);
        builder.UseSetting("Redis:InstanceName", "Test:");
        // Sipariş sonrası olay yayını gerçek AMQP’ye bağlı kalmasın (Publish hatası → 400 görünür).
        builder.UseSetting("RabbitMq:Transport", "InMemory");
        builder.UseSetting("RabbitMq:Host", _rabbitHost);
        builder.UseSetting("RabbitMq:Port", _rabbitPort.ToString());
        builder.UseSetting("RabbitMq:VirtualHost", "/");
        builder.UseSetting("RabbitMq:Username", "ecommerce");
        builder.UseSetting("RabbitMq:Password", "ecommerce_test");
        builder.UseSetting("Checkout:SimulatedPaymentDeclinePercent", "0");
        builder.UseSetting("Jwt:Key", "integration-test-secret-key-at-least-32-chars-long!!");
        builder.UseSetting("Jwt:Issuer", "EcommerceBackend");
        builder.UseSetting("Jwt:Audience", "EcommerceUsers");
        builder.UseSetting("Jwt:ExpirationInMinutes", "120");
        builder.UseSetting("Logging:LogLevel:Default", "Warning");
        builder.UseSetting("Logging:LogLevel:Microsoft.AspNetCore", "Warning");
        builder.UseSetting("Logging:LogLevel:Microsoft.EntityFrameworkCore", "Warning");
    }
}
