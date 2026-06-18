using System.ComponentModel.DataAnnotations;

namespace Decidi.Web.Models;

public class ConversationDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string OtherUserId { get; set; } = string.Empty;
    public string OtherUserName { get; set; } = string.Empty;
    public string? OtherUserAvatarUrl { get; set; }
    public string? LastMessage { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public class MessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool WasRedacted { get; set; }
}

public class SendMessageRequest
{
    [Required]
    public Guid ConversationId { get; set; }

    [Required(ErrorMessage = "Mensagem é obrigatória")]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}
