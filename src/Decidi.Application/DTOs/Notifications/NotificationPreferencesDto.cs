namespace Decidi.Application.DTOs.Notifications;

public class NotificationPreferencesDto
{
    public bool EmailProposalReceived { get; set; } = true;
    public bool EmailProposalAccepted { get; set; } = true;
    public bool EmailProposalRejected { get; set; } = true;
    public bool EmailProjectCompleted { get; set; } = true;
    public bool EmailChatOfflineDigest { get; set; } = true;
}
