using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Decidi.Infrastructure.Repositories;

public class NotificationPreferencesRepository(AppDbContext context)
    : Repository<NotificationPreferences>(context), INotificationPreferencesRepository
{
    public async Task<NotificationPreferences?> GetByUserIdAsync(string userId)
    {
        // Usa o unique index (UserId) já configurado em NotificationPreferencesConfiguration.
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId);
    }
}
