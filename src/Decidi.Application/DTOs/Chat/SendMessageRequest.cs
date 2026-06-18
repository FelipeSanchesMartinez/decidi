using System.ComponentModel.DataAnnotations;

namespace Decidi.Application.DTOs.Chat;

public class SendMessageRequest
{
    [Required]
    public Guid ConversationId { get; set; }

    [Required(ErrorMessage = "Mensagem é obrigatória")]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
}
