using Decidi.Domain.Entities;

namespace Decidi.Domain.Interfaces;

public interface INotificationRepository : IRepository<Notification>
{
    Task<int> GetUnreadCountAsync(string userId);
    Task<IEnumerable<Notification>> GetByUserAsync(string userId, int take = 20);
    /// <summary>Marca uma notificação específica do usuário como lida (no-op se não for dele).</summary>
    Task<bool> MarkAsReadAsync(Guid id, string userId);
    /// <summary>Marca todas as não-lidas do usuário como lidas.</summary>
    Task MarkAllAsReadAsync(string userId);
}
