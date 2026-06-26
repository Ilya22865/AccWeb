using AuthService.Data;
using AuthService.DTO.Auth;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ITokenGenerator _tokenGenerator;

    public AuthService(AppDbContext context, ITokenGenerator tokenGenerator)
    {
        _context = context;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<LoginResponse> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email)
            ?? throw new Exception("Invalid email or password");

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new Exception("Invalid email or password");

        var token = await _tokenGenerator.GenerateTokenAsync(user.Id, user.Email, user.FullName, user.Role);

        return new LoginResponse(token, "", user.Id, user.FullName, user.Role);
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            throw new Exception("User already exists");

        var user = new User
        {
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = UserRole.User
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = await _tokenGenerator.GenerateTokenAsync(user.Id, user.Email, user.FullName, user.Role);

        return new RegisterResponse(token, user.Id, user.FullName, user.Role);
    }
}
