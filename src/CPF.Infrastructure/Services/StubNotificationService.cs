using CPF.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace CPF.Infrastructure.Services;

public class StubNotificationService : INotificationService
{
    private readonly ILogger<StubNotificationService> _logger;

    public StubNotificationService(ILogger<StubNotificationService> logger) => _logger = logger;

    public Task SendPanicNotificationAsync(Guid alertId, double latitude, double longitude, string triggeredByUser)
    {
        _logger.LogWarning(
            "[DEV STUB] Panic notification — AlertId: {AlertId}, User: {User}, Location: ({Lat}, {Lng})",
            alertId, triggeredByUser, latitude, longitude);
        return Task.CompletedTask;
    }
}
