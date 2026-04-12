using CPF.Domain.Enums;

namespace CPF.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public UserRole Role { get; set; } = UserRole.Member;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<PanicAlert> PanicAlerts { get; set; } = [];
    public ICollection<Shift> Shifts { get; set; } = [];
    public ICollection<Payment> Payments { get; set; } = [];
    public ICollection<Post> Posts { get; set; } = [];
}
