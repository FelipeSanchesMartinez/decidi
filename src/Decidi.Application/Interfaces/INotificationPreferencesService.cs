using Decidi.Application.DTOs.Notifications;

namespace Decidi.Application.Interfaces;

public interface INotificationPreferencesService
{
    Task<NotificationPreferencesDto> GetForUserAsync(string userId);
    Task<NotificationPreferencesDto> UpdateForUserAsync(string userId, NotificationPreferencesDto prefs);
}
