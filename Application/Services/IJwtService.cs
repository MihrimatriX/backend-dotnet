namespace EcommerceBackend.Application.Services
{
    public interface IJwtService
    {
        /// <param name="role">JWT role claim (<c>User</c> veya <c>Admin</c>).</param>
        string GenerateToken(string email, long userId, string role);
    }
}
