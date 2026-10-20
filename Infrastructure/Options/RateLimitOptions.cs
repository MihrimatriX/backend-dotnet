namespace EcommerceBackend.Infrastructure.Options;

/// <summary>IP başına sabit pencere hız limiti (§1.4).</summary>
public sealed class RateLimitOptions
{
    public const string SectionName = "RateLimit";

    /// <summary>Dakikada izin verilen istek sayısı; 0 veya negatif = kapalı.</summary>
    public int RequestsPerMinute { get; set; } = 300;
}
