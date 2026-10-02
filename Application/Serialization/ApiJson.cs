using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using EcommerceBackend.Application.DTOs;

namespace EcommerceBackend.Application.Serialization;

/// <summary>
/// API'nin tek JSON yapılandırması: MVC, hata middleware'leri, JWT olayları ve ayar dışa aktarımı aynı ayarları kullanır.
/// camelCase, büyük/küçük harf duyarsız okuma, nesne içindeki null alanlar yazılır (§1 "JSON"), tarihler UTC <c>Z</c>.
/// </summary>
public static class ApiJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    public static void Configure(JsonSerializerOptions options)
    {
        options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.PropertyNameCaseInsensitive = true;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.Converters.Add(new UtcDateTimeConverter());
        options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers = { OmitNullEnvelopeData },
        };
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Configure(options);
        options.MakeReadOnly();
        return options;
    }

    /// <summary>
    /// Zarftaki <c>data</c> null ise yazılmaz. <c>[JsonIgnore(WhenWritingNull)]</c> değer tipli <c>T</c>
    /// (bool, int, decimal) için serileştirmede hata verdiğinden kural burada uygulanır.
    /// </summary>
    private static void OmitNullEnvelopeData(JsonTypeInfo typeInfo)
    {
        if (!typeInfo.Type.IsGenericType || typeInfo.Type.GetGenericTypeDefinition() != typeof(BaseResponseDto<>))
            return;

        foreach (var property in typeInfo.Properties)
        {
            if (property.Name == "data")
                property.ShouldSerialize = static (_, value) => value is not null;
        }
    }
}
