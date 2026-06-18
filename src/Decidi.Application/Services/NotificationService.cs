using Decidi.Application.DTOs.Notifications;
using Decidi.Application.Interfaces;
using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;

namespace Decidi.Application.Services;

public class NotificationService(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    INotificationPusher? pusher = null) : INotificationService
{
    public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, int take = 20)
    {
        var items = await notificationRepository.GetByUserAsync(userId, take);
        return items.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type,
            Title = n.Title,
            Message = n.Message,
            Link = n.Link,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public Task<int> GetUnreadCountAsync(string userId) =>
        notificationRepository.GetUnreadCountAsync(userId);

    public async Task MarkAsReadAsync(Guid id, string userId)
    {
        var changed = await notificationRepository.MarkAsReadAsync(id, userId);
        if (changed) await unitOfWork.SaveChangesAsync();
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        await notificationRepository.MarkAllAsReadAsync(userId);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task CreateAsync(string userId, string type, string title, string message, string? link = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            Link = link
        };
        await notificationRepository.AddAsync(notification);
        await unitOfWork.SaveChangesAsync();

        // Push em tempo real para o usuário (best-effort).
        if (pusher is not null)
        {
            try
            {
                await pusher.PushAsync(userId, new NotificationDto
                {
                    Id = notification.Id,
                    Type = notification.Type,
                    Title = notification.Title,
                    Message = notification.Message,
                    Link = notification.Link,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt
                });
            }
            catch { /* não bloqueia o evento principal */ }
        }
    }
}
