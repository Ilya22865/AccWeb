using AuthService.Models;

namespace AuthService.DTO.Auth
{
    public record RegisterResponse(
        int UserId,
        string FullName,
        UserRole Role
    );
    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string FullName { get; set; } = null!;
    }
}