namespace CPF.Application.Interfaces;

public interface IShiftService
{
    Task<IReadOnlyList<ShiftDto>> GetShiftsAsync(Guid userId);
    Task<ShiftActionResult> CheckInAsync(Guid shiftId, Guid userId, double? latitude, double? longitude);
    Task<ShiftActionResult> CheckOutAsync(Guid shiftId, Guid userId);
}

public record ShiftDto(
    Guid Id,
    Guid UserId,
    DateTime ScheduledStartUtc,
    DateTime ScheduledEndUtc,
    DateTime? CheckInUtc,
    DateTime? CheckOutUtc);

public record ShiftActionResult(bool Success, string? Error = null);
