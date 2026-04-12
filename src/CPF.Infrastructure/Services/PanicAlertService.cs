using CPF.Application.Interfaces;
using CPF.Domain.Entities;
using CPF.Domain.Enums;
using CPF.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CPF.Infrastructure.Services;

public class PanicAlertService : IPanicAlertService
{
    private readonly CpfDbContext _db;
    private readonly INotificationService _notifications;

    public PanicAlertService(CpfDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<PanicAlertResult> TriggerAsync(Guid userId, double latitude, double longitude, string? description)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null)
            return new PanicAlertResult(false, Error: "User not found.");

        var alert = new PanicAlert
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Latitude = latitude,
            Longitude = longitude,
            Description = description,
            Status = PanicAlertStatus.Active,
            TriggeredAtUtc = DateTime.UtcNow
        };

        _db.PanicAlerts.Add(alert);
        await _db.SaveChangesAsync();

        // Fire-and-forget the notification — don't block the panic response
        _ = Task.Run(() => _notifications.SendPanicNotificationAsync(alert.Id, latitude, longitude, user.FullName));

        return new PanicAlertResult(true, alert.Id);
    }

    public async Task<IReadOnlyList<PanicAlertDto>> GetActiveAlertsAsync()
    {
        return await _db.PanicAlerts
            .Include(a => a.User)
            .Where(a => a.Status == PanicAlertStatus.Active)
            .OrderByDescending(a => a.TriggeredAtUtc)
            .Select(a => new PanicAlertDto(
                a.Id,
                a.UserId,
                a.User.FullName,
                a.Latitude,
                a.Longitude,
                a.Description,
                a.Status.ToString(),
                a.TriggeredAtUtc))
            .ToListAsync();
    }

    public async Task ResolveAsync(Guid alertId, Guid resolvedByUserId)
    {
        var alert = await _db.PanicAlerts.FindAsync(alertId);
        if (alert is null) return;

        alert.Status = PanicAlertStatus.Resolved;
        alert.ResolvedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
