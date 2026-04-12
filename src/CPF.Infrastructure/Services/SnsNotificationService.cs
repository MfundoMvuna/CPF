using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using CPF.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CPF.Infrastructure.Services;

public class SnsNotificationService : INotificationService
{
    private readonly IAmazonSimpleNotificationService _sns;
    private readonly ILogger<SnsNotificationService> _logger;
    private readonly string _topicArn;

    public SnsNotificationService(
        IAmazonSimpleNotificationService sns,
        ILogger<SnsNotificationService> logger)
    {
        _sns = sns;
        _logger = logger;
        // Set via environment variable — never hardcoded
        _topicArn = Environment.GetEnvironmentVariable("SNS_PANIC_TOPIC_ARN")
            ?? throw new InvalidOperationException("SNS_PANIC_TOPIC_ARN environment variable is not set.");
    }

    public async Task SendPanicNotificationAsync(Guid alertId, double latitude, double longitude, string triggeredByUser)
    {
        var message = $"🚨 PANIC ALERT from {triggeredByUser}!\n" +
                      $"Location: {latitude}, {longitude}\n" +
                      $"Alert ID: {alertId}\n" +
                      $"Time: {DateTime.UtcNow:u}";

        try
        {
            var request = new PublishRequest
            {
                TopicArn = _topicArn,
                Subject = "CPF Panic Alert",
                Message = message,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>
                {
                    ["alertId"] = new() { DataType = "String", StringValue = alertId.ToString() },
                    ["latitude"] = new() { DataType = "Number", StringValue = latitude.ToString("F6") },
                    ["longitude"] = new() { DataType = "Number", StringValue = longitude.ToString("F6") }
                }
            };

            var response = await _sns.PublishAsync(request);
            _logger.LogInformation("Panic notification sent. MessageId: {MessageId}", response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send panic notification for alert {AlertId}", alertId);
            // Don't rethrow — panic was already saved, notification failure shouldn't break the flow
        }
    }
}
