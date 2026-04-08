using System.Net;
using System.Text.Json;
using EcommerceBackend.IntegrationTests.Support;
using Xunit;

namespace EcommerceBackend.IntegrationTests.Health;

[Collection(nameof(IntegrationCollection))]
[Trait("Category", "Integration")]
public sealed class HealthAndInfraTests
{
    private readonly HttpClient _client;

    public HealthAndInfraTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
    }

    [Fact]
    public async Task Root_returns_running_message()
    {
        var text = await _client.GetStringAsync("/");
        Assert.Contains("E-Commerce API", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Liveness_health_returns_ok()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = (await response.Content.ReadAsStringAsync()).Trim();
        Assert.Equal("OK", body);
    }

    [Fact]
    public async Task Api_health_reports_postgres_redis_and_rabbit()
    {
        var response = await _client.GetAsync("/api/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync();
        using var doc = await JsonDocument.ParseAsync(stream);
        var root = doc.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());

        var entries = root.GetProperty("entries");
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        switch (entries.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var prop in entries.EnumerateObject())
                    names.Add(prop.Name);
                break;
            case JsonValueKind.Array:
                foreach (var item in entries.EnumerateArray())
                {
                    if (item.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String)
                        names.Add(n.GetString()!);
                }
                break;
            default:
                Assert.Fail($"Unexpected health entries JSON: {entries.ValueKind}");
                break;
        }

        Assert.Contains("self", names);
        Assert.Contains("redis", names);
        // rabbitmq health girdisi yalnızca RabbitMq:Transport=RabbitMq iken eklenir; test hostu InMemory kullanır.
    }
}
