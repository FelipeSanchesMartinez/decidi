using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Decidi.Infrastructure.Repositories;

public class NotificationRepository(AppDbContext context)
    : Repository<Notification>(context), INotificationRepository
{
    public async Task<int> GetUnreadCountAsync(string userId)
    {
        // Usa o indice composto (UserId, IsRead) — index-seek puro.
        return await _dbSet.CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task<IEnumerable<Notification>> GetByUserAsync(string userId, int take = 20)
    {
        // Usa o indice (UserId, CreatedAt) do Sprint 7.1.
        return await _dbSet
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<bool> MarkAsReadAsync(Guid id, string userId)
    {
        var notif = await _dbSet.FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
        if (notif is null) return false;
        if (notif.IsRead) return true;
        notif.IsRead = true;
        return true;
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        // UPDATE em lote server-side — sem hidratar entidades nem encher o change tracker.
        // Usa o índice (UserId, IsRead) do Sprint 7.1.
        await _dbSet
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }
}
