namespace Decidi.Application.DTOs.Chat;

public class MessageDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    // Sinaliza ao cliente que a mensagem teve contatos removidos pelo antifraude.
    public bool WasRedacted { get; set; }
}
