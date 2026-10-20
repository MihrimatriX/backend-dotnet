namespace EcommerceBackend.Application.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Type { get; set; } = "Bearer";
        public long UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsEmailVerified { get; set; }
        /// <summary><c>Admin</c> veya <c>User</c> (token'daki <c>role</c> claim'i ile aynı).</summary>
        public string Role { get; set; } = string.Empty;
    }
}
