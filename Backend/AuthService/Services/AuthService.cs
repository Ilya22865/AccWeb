using AuthService.Data;
using AuthService.DTO.Auth;
using AuthService.Models;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;

    public AuthService(AppDbContext context)
    {
        _context = context;
    }

    public Task<LoginResponse> LoginAsync(LoginDto loginDto)
    {
        throw new NotImplementedException();
    }

    public Task<RegisterResponse> RegisterAsync(RegisterDto registerDto)
    {
        throw new NotImplementedException();
    }
}
