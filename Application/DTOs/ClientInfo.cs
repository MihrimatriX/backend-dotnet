namespace EcommerceBackend.Application.DTOs
{
    /// <summary>İsteği yapan istemcinin bilgileri (giriş geçmişi için).</summary>
    public sealed record ClientInfo(string? IpAddress, string? UserAgent);
}
