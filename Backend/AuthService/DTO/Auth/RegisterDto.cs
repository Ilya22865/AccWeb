using AuthService.Models;
using System.ComponentModel.DataAnnotations;

namespace AuthService.DTO.Auth
{
    public record RegisterResponse(
        string Token,
        int UserId,
        string FullName,
        UserRole Role
    );
    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        [Required]
        [RegularExpression(@"^[А-Яа-яA-Za-z]+\s[А-Яа-яA-Za-z]+$", 
                ErrorMessage = "FullName must be in format 'First Last'")]
        public string FullName { get; set; } = null!;
    }
}