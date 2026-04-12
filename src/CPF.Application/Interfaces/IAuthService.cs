using CPF.Domain.Entities;

namespace CPF.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string fullName, string email, string password, string phoneNumber);
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AuthResult> RefreshTokenAsync(string accessToken, string refreshToken);
}

public record AuthResult(
    bool Success,
    string? AccessToken = null,
    string? RefreshToken = null,
    string? Error = null);
