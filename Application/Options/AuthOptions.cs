namespace EcommerceBackend.Application.Options;

/// <summary>Rol kuralı (§2): e-posta bu listedeyse <c>Admin</c>, değilse <c>User</c>. Kayıt ve giriş aynı kuralı kullanır.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    public string[] AdminEmails { get; set; } = ["admin@example.com", "manager@shop.demo"];

    public string ResolveRole(string normalizedEmail) =>
        AdminEmails.Any(a => string.Equals(a?.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase))
            ? UserRoles.Admin
            : UserRoles.User;
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string User = "User";
}
