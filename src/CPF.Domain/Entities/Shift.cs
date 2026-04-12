namespace CPF.Domain.Entities;

public class Shift
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime ScheduledStartUtc { get; set; }
    public DateTime ScheduledEndUtc { get; set; }
    public DateTime? CheckInUtc { get; set; }
    public DateTime? CheckOutUtc { get; set; }
    public double? CheckInLatitude { get; set; }
    public double? CheckInLongitude { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = default!;
}
