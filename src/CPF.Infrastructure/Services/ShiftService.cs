using CPF.Application.Interfaces;
using CPF.Domain.Entities;
using CPF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPF.Infrastructure.Services;

public class ShiftService : IShiftService
{
    private readonly CpfDbContext _db;

    public ShiftService(CpfDbContext db) => _db = db;

    public async Task<IReadOnlyList<ShiftDto>> GetShiftsAsync(Guid userId)
    {
        return await _db.Shifts
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.ScheduledStartUtc)
            .Select(s => new ShiftDto(
                s.Id,
                s.UserId,
                s.ScheduledStartUtc,
                s.ScheduledEndUtc,
                s.CheckInUtc,
                s.CheckOutUtc))
            .ToListAsync();
    }

    public async Task<ShiftActionResult> CheckInAsync(Guid shiftId, Guid userId, double? latitude, double? longitude)
    {
        var shift = await _db.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId && s.UserId == userId);
        if (shift is null)
            return new ShiftActionResult(false, "Shift not found.");

        if (shift.CheckInUtc.HasValue)
            return new ShiftActionResult(false, "Already checked in.");

        shift.CheckInUtc = DateTime.UtcNow;
        shift.CheckInLatitude = latitude;
        shift.CheckInLongitude = longitude;
        await _db.SaveChangesAsync();

        return new ShiftActionResult(true);
    }

    public async Task<ShiftActionResult> CheckOutAsync(Guid shiftId, Guid userId)
    {
        var shift = await _db.Shifts.FirstOrDefaultAsync(s => s.Id == shiftId && s.UserId == userId);
        if (shift is null)
            return new ShiftActionResult(false, "Shift not found.");

        if (!shift.CheckInUtc.HasValue)
            return new ShiftActionResult(false, "Must check in before checking out.");

        if (shift.CheckOutUtc.HasValue)
            return new ShiftActionResult(false, "Already checked out.");

        shift.CheckOutUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return new ShiftActionResult(true);
    }
}
