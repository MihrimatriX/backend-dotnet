using System.Text.Json;

namespace EcommerceBackend.IntegrationTests.Support;

internal static class TestJson
{
    public static JsonSerializerOptions Options { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
