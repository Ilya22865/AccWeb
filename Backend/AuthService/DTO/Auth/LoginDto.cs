using AuthService.Models;
using System.ComponentModel.DataAnnotations;
namespace AuthService.DTO.Auth
{
    public record LoginResponse(
        string Token,
        string RefreshToken,
        int UserId,
        string FullName,
        UserRole Role
    );
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(8)]
        public string Password { get; set; } = null!;
    }
}