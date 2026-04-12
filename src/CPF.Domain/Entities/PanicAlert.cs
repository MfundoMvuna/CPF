using CPF.Domain.Enums;

namespace CPF.Domain.Entities;

public class PanicAlert
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Description { get; set; }
    public PanicAlertStatus Status { get; set; } = PanicAlertStatus.Active;
    public DateTime TriggeredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }

    public User User { get; set; } = default!;
}
