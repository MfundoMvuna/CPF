using System.ComponentModel.DataAnnotations;

namespace CPF.Application.DTOs;

public record RegisterRequest(
    [Required, StringLength(100)] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required, Phone] string PhoneNumber);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record RefreshTokenRequest(
    [Required] string AccessToken,
    [Required] string RefreshToken);
