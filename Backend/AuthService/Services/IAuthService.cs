using AuthService.DTO.Auth;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<LoginDto> LoginAsync(LoginDto loginDto);
        Task<RegisterDto> RegisterAsync(RegisterDto registerDto);
    }
}