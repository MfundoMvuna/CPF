using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CPF.Application.Interfaces;
using CPF.Domain.Entities;
using CPF.Infrastructure.Data;
using CPF.Infrastructure.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CPF.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly CpfDbContext _db;
    private readonly JwtSettings _jwt;

    public AuthService(CpfDbContext db, IOptions<JwtSettings> jwt)
    {
        _db = db;
        _jwt = jwt.Value;
    }

    public async Task<AuthResult> RegisterAsync(string fullName, string email, string password, string phoneNumber)
    {
        if (await _db.Users.AnyAsync(u => u.Email == email))
            return new AuthResult(false, Error: "Email already registered.");

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            PhoneNumber = phoneNumber
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return GenerateTokens(user);
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return new AuthResult(false, Error: "Invalid email or password.");

        return GenerateTokens(user);
    }

    public async Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var principal = GetPrincipalFromExpiredToken(accessToken);
        if (principal is null)
            return new AuthResult(false, Error: "Invalid access token.");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
            return new AuthResult(false, Error: "Invalid token claims.");

        var user = await _db.Users.FindAsync(userId);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryUtc <= DateTime.UtcNow)
            return new AuthResult(false, Error: "Invalid or expired refresh token.");

        return GenerateTokens(user);
    }

    private AuthResult GenerateTokens(User user)
    {
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryUtc = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays);
        _db.SaveChanges();

        return new AuthResult(true, accessToken, refreshToken);
    }

    private string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var validation = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = _jwt.Issuer,
            ValidAudience = _jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret)),
            ValidateLifetime = false // allow expired tokens for refresh
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, validation, out var securityToken);
            if (securityToken is not JwtSecurityToken jwt || !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
