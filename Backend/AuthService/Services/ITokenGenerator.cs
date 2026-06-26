using AuthService.Models;

namespace AuthService.Services
{
    public interface ITokenGenerator
    {
        Task<string> GenerateTokenAsync(int id, string email, string fullName, UserRole role);
    }
}