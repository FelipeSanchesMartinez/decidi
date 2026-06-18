using Decidi.Domain.Entities;

namespace Decidi.Domain.Interfaces;

public interface INotificationPreferencesRepository : IRepository<NotificationPreferences>
{
    Task<NotificationPreferences?> GetByUserIdAsync(string userId);
}
