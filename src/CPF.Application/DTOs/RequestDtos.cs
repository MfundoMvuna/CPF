using System.ComponentModel.DataAnnotations;

namespace CPF.Application.DTOs;

public record TriggerPanicRequest(
    [Required, Range(-90, 90)] double Latitude,
    [Required, Range(-180, 180)] double Longitude,
    string? Description);

public record CreatePaymentRequest(
    [Required, Range(100, 100_000_00)] int AmountInCents,
    [Required, StringLength(200)] string Description);

public record CreatePostRequest(
    [Required, StringLength(200)] string Title,
    [Required, StringLength(5000)] string Content,
    string? ImageUrl);

public record CheckInRequest(
    double? Latitude,
    double? Longitude);
