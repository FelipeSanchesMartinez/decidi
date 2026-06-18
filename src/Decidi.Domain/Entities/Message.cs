using Decidi.Domain.Common;

namespace Decidi.Domain.Entities;

public class Message : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }

    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser Sender { get; set; } = null!;

    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;
}
