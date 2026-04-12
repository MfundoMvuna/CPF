namespace CPF.Application.Interfaces;

public interface IPanicAlertService
{
    Task<PanicAlertResult> TriggerAsync(Guid userId, double latitude, double longitude, string? description);
    Task<IReadOnlyList<PanicAlertDto>> GetActiveAlertsAsync();
    Task ResolveAsync(Guid alertId, Guid resolvedByUserId);
}

public record PanicAlertResult(bool Success, Guid? AlertId = null, string? Error = null);

public record PanicAlertDto(
    Guid Id,
    Guid UserId,
    string UserFullName,
    double Latitude,
    double Longitude,
    string? Description,
    string Status,
    DateTime TriggeredAtUtc);
