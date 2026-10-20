using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace EcommerceBackend.Application.Serialization;

/// <summary>
/// Tüm <see cref="DateTime"/> değerlerini ISO-8601 UTC (<c>Z</c> sonekli) yazar; okurken <c>Z</c>/ofsetli değerleri
/// UTC'ye çevirir, ofsetsiz değerleri UTC kabul eder (§1 "Tarih"). SQLite'tan gelen
/// <see cref="DateTimeKind.Unspecified"/> değerler UTC sayılır. <c>DateTime?</c> için de otomatik kullanılır.
/// </summary>
public sealed class UtcDateTimeConverter : JsonConverter<DateTime>
{
    private const string Format = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Tarih değeri metin olmalıdır.");

        var text = reader.GetString();
        if (!DateTime.TryParse(
                text,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var value))
        {
            throw new JsonException($"Geçersiz tarih: '{text}'.");
        }

        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteStringValue(ToUtc(value).ToString(Format, CultureInfo.InvariantCulture));

    public static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
    };
}
