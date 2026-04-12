namespace CPF.Application.Interfaces;

public interface INotificationService
{
    Task SendPanicNotificationAsync(Guid alertId, double latitude, double longitude, string triggeredByUser);
}
