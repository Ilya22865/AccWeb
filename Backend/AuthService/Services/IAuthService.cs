using AuthService.DTO.Auth;

namespace AuthService.Services;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginDto loginDto);
    Task<RegisterResponse> RegisterAsync(RegisterDto registerDto);
}
