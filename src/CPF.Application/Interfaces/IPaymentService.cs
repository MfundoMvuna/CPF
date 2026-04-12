namespace CPF.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentCreateResult> CreatePaymentAsync(Guid userId, int amountInCents, string description);
    Task ProcessWebhookAsync(string payload, string signature);
}

public record PaymentCreateResult(
    bool Success,
    Guid? PaymentId = null,
    string? CheckoutUrl = null,
    string? Error = null);
