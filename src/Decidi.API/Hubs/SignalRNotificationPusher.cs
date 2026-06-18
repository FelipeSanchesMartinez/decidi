using Decidi.Application.DTOs.Notifications;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Decidi.API.Hubs;

public class SignalRNotificationPusher(IHubContext<NotificationHub> hub) : INotificationPusher
{
    public Task PushAsync(string userId, NotificationDto notification)
    {
        if (string.IsNullOrEmpty(userId)) return Task.CompletedTask;
        return hub.Clients.Group(NotificationHub.GroupName(userId))
            .SendAsync("ReceiveNotification", notification);
    }
}
