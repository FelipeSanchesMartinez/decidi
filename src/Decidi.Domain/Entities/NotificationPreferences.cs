using Decidi.Domain.Common;

namespace Decidi.Domain.Entities;

/// <summary>1:1 com ApplicationUser. Default = tudo true (não quebra comportamento atual).</summary>
public class NotificationPreferences : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public bool EmailProposalReceived { get; set; } = true;
    public bool EmailProposalAccepted { get; set; } = true;
    public bool EmailProposalRejected { get; set; } = true;
    public bool EmailProjectCompleted { get; set; } = true;
    public bool EmailChatOfflineDigest { get; set; } = true;
}
