using Decidi.Application.DTOs.Notifications;

namespace Decidi.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, int take = 20);
    Task<int> GetUnreadCountAsync(string userId);
    Task MarkAsReadAsync(Guid id, string userId);
    Task MarkAllAsReadAsync(string userId);
    Task CreateAsync(string userId, string type, string title, string message, string? link = null);
}
