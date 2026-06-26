using System.Security.Cryptography;
using AuthService.Data;
using AuthService.DTO.Auth;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IEmailValidator _emailValidator;

    public AuthService(AppDbContext context, IConfiguration configuration, ITokenGenerator tokenGenerator, IEmailValidator emailValidator)
    {
        _context = context;
        _configuration = configuration;
        _tokenGenerator = tokenGenerator;
        _emailValidator = emailValidator;
    }
    private async Task<RefreshToken> ValidateRefreshTokenAsync(string refreshTokenStr)
    {
        if (string.IsNullOrEmpty(refreshTokenStr))
            throw new Exception("Refresh token is required");

        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshTokenStr)
            ?? throw new Exception("Invalid refresh token");

        if (refreshToken.IsRevoked)
            throw new Exception("Refresh token revoked");

        if (refreshToken.Expires < DateTime.UtcNow)
            throw new Exception("Refresh token expired");

        return refreshToken;
    }
    
    public async Task<RefreshTokenResponse> RefreshAsync(RefreshTokenRequest request)
    {
        var refreshToken = await ValidateRefreshTokenAsync(request.RefreshToken);

        var user = await _context.Users.FindAsync(refreshToken.UserId)
            ?? throw new Exception("User not found");
            
        var accessToken = await _tokenGenerator.GenerateTokenAsync(user.Id, user.Email, user.FullName, user.Role);
        var newRefreshTokenStr = GenerateRefreshToken();

        refreshToken.IsRevoked = true;

        _context.RefreshTokens.Add(new RefreshToken
        {
            Token = newRefreshTokenStr,
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
        });
        await _context.SaveChangesAsync();

        return new RefreshTokenResponse(accessToken, newRefreshTokenStr);
    }

    public async Task<LoginResponse> LoginAsync(LoginDto loginDto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email)
            ?? throw new Exception("User not found");

        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            throw new Exception("Invalid password");

        var token = await _tokenGenerator.GenerateTokenAsync(user.Id, user.Email, user.FullName, user.Role);
        string refreshTokenStr = GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            Token = refreshTokenStr,
            UserId = user.Id,
            Expires = DateTime.UtcNow.AddDays(30),
            CreatedAt = DateTime.UtcNow,
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return new LoginResponse(token, refreshTokenStr, user.Id, user.FullName, user.Role);
    }
    private static string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }
    public async Task<RegisterResponse> RegisterAsync(RegisterDto registerDto)
    {
        if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            throw new Exception("User already exists");

        string? apiKey = _configuration["Hunter:ApiKey"];
        bool isValidEmail = await _emailValidator.IsValid(registerDto.Email, apiKey ?? "");
        if (!isValidEmail)
            throw new Exception("Invalid email");

        var user = new User
        {
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Role = UserRole.User
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        if(user.Id == 1) user.Role = UserRole.Admin;
        await _context.SaveChangesAsync();
        
        var token = await _tokenGenerator.GenerateTokenAsync(user.Id, user.Email, user.FullName, user.Role);

        return new RegisterResponse(token, user.Id, user.FullName, user.Role);
    }
}
