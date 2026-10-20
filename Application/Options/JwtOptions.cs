namespace EcommerceBackend.Application.Options;

/// <summary>JWT ayarları (§2). Üretimde <c>Jwt__Key</c> ortam değişkeniyle değiştirilmelidir.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Spring ikiziyle ortak geliştirme anahtarı.</summary>
    public const string DevelopmentKey = "mySecretKeyThatIsAtLeast256BitsLongForJWTTokenSecurity";

    public string Key { get; set; } = DevelopmentKey;

    public string Issuer { get; set; } = "EcommerceBackend";

    public string Audience { get; set; } = "EcommerceUsers";

    public int ExpirationInMinutes { get; set; } = 1440;
}
