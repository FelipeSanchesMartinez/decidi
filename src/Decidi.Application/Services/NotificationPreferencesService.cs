using Decidi.Application.DTOs.Notifications;
using Decidi.Application.Interfaces;
using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;

namespace Decidi.Application.Services;

public class NotificationPreferencesService(
    INotificationPreferencesRepository repository,
    IUnitOfWork unitOfWork) : INotificationPreferencesService
{
    public async Task<NotificationPreferencesDto> GetForUserAsync(string userId)
    {
        var entity = await GetOrCreateAsync(userId);
        return ToDto(entity);
    }

    public async Task<NotificationPreferencesDto> UpdateForUserAsync(string userId, NotificationPreferencesDto prefs)
    {
        var entity = await GetOrCreateAsync(userId);
        entity.EmailProposalReceived = prefs.EmailProposalReceived;
        entity.EmailProposalAccepted = prefs.EmailProposalAccepted;
        entity.EmailProposalRejected = prefs.EmailProposalRejected;
        entity.EmailProjectCompleted = prefs.EmailProjectCompleted;
        entity.EmailChatOfflineDigest = prefs.EmailChatOfflineDigest;
        repository.Update(entity);
        await unitOfWork.SaveChangesAsync();
        return ToDto(entity);
    }

    private async Task<NotificationPreferences> GetOrCreateAsync(string userId)
    {
        var existing = await repository.GetByUserIdAsync(userId);
        if (existing is not null) return existing;

        var prefs = new NotificationPreferences { UserId = userId };
        await repository.AddAsync(prefs);
        await unitOfWork.SaveChangesAsync();
        return prefs;
    }

    private static NotificationPreferencesDto ToDto(NotificationPreferences e) => new()
    {
        EmailProposalReceived = e.EmailProposalReceived,
        EmailProposalAccepted = e.EmailProposalAccepted,
        EmailProposalRejected = e.EmailProposalRejected,
        EmailProjectCompleted = e.EmailProjectCompleted,
        EmailChatOfflineDigest = e.EmailChatOfflineDigest
    };
}
