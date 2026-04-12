using CPF.Domain.Enums;

namespace CPF.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int AmountInCents { get; set; }
    public string Currency { get; set; } = "ZAR";
    public string? YocoChargeId { get; set; }
    public string? YocoCheckoutId { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? Description { get; set; }
    public string? WebhookPayload { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }

    public User User { get; set; } = default!;
}
