using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CPF.Application.Interfaces;
using CPF.Domain.Enums;
using CPF.Infrastructure.Data;
using CPF.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CPF.Infrastructure.Services;

public class YocoPaymentService : IPaymentService
{
    private readonly CpfDbContext _db;
    private readonly HttpClient _http;
    private readonly YocoSettings _settings;
    private readonly ILogger<YocoPaymentService> _logger;

    public YocoPaymentService(
        CpfDbContext db,
        HttpClient http,
        IOptions<YocoSettings> settings,
        ILogger<YocoPaymentService> logger)
    {
        _db = db;
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaymentCreateResult> CreatePaymentAsync(Guid userId, int amountInCents, string description)
    {
        var payment = new Domain.Entities.Payment
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AmountInCents = amountInCents,
            Description = description,
            Status = PaymentStatus.Pending
        };

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        try
        {
            var requestBody = new
            {
                amount = amountInCents,
                currency = "ZAR",
                metadata = new { paymentId = payment.Id.ToString(), description }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.BaseUrl}/checkouts")
            {
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.SecretKey);

            var response = await _http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Yoco checkout creation failed: {StatusCode} {Body}", response.StatusCode, responseBody);
                payment.Status = PaymentStatus.Failed;
                await _db.SaveChangesAsync();
                return new PaymentCreateResult(false, payment.Id, Error: "Payment creation failed.");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var checkoutId = doc.RootElement.GetProperty("id").GetString();
            var redirectUrl = doc.RootElement.GetProperty("redirectUrl").GetString();

            payment.YocoCheckoutId = checkoutId;
            await _db.SaveChangesAsync();

            return new PaymentCreateResult(true, payment.Id, redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Yoco checkout for payment {PaymentId}", payment.Id);
            return new PaymentCreateResult(false, payment.Id, Error: "An error occurred creating the payment.");
        }
    }

    public async Task ProcessWebhookAsync(string payload, string signature)
    {
        // Validate webhook signature using HMAC-SHA256
        if (!ValidateWebhookSignature(payload, signature))
        {
            _logger.LogWarning("Invalid Yoco webhook signature.");
            throw new UnauthorizedAccessException("Invalid webhook signature.");
        }

        using var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var eventType = root.GetProperty("type").GetString();
        var checkoutId = root.GetProperty("payload").GetProperty("metadata").GetProperty("paymentId").GetString();

        if (!Guid.TryParse(checkoutId, out var paymentId))
        {
            _logger.LogWarning("Webhook contained invalid paymentId: {PaymentId}", checkoutId);
            return;
        }

        var payment = await _db.Payments.FindAsync(paymentId);
        if (payment is null)
        {
            _logger.LogWarning("Payment not found for webhook: {PaymentId}", paymentId);
            return;
        }

        payment.WebhookPayload = payload;

        switch (eventType)
        {
            case "payment.succeeded":
                payment.Status = PaymentStatus.Succeeded;
                payment.CompletedAtUtc = DateTime.UtcNow;
                payment.YocoChargeId = root.GetProperty("payload").GetProperty("id").GetString();
                break;
            case "payment.failed":
                payment.Status = PaymentStatus.Failed;
                payment.CompletedAtUtc = DateTime.UtcNow;
                break;
            default:
                _logger.LogInformation("Unhandled Yoco webhook event: {EventType}", eventType);
                return;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Processed Yoco webhook {EventType} for payment {PaymentId}", eventType, paymentId);
    }

    private bool ValidateWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(signature))
            return false;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.WebhookSecret));
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computedSignature = Convert.ToHexString(computedHash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSignature),
            Encoding.UTF8.GetBytes(signature));
    }
}
