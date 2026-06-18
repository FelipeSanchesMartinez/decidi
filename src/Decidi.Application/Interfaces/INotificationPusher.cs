using Decidi.Application.DTOs.Notifications;

namespace Decidi.Application.Interfaces;

/// <summary>
/// Envia notificações em tempo real para o usuário (via SignalR ou push).
/// Implementação no Infrastructure/API para evitar dependência da camada Application no SignalR.
/// </summary>
public interface INotificationPusher
{
    Task PushAsync(string userId, NotificationDto notification);
}
