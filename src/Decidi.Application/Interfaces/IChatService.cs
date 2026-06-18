using Decidi.Application.DTOs.Chat;

namespace Decidi.Application.Interfaces;

public interface IChatService
{
    Task<ConversationDto> GetOrCreateConversationAsync(Guid projectId, string freelancerId, string currentUserId);
    Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(string userId);
    Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, string userId);
    Task<MessageDto> SendMessageAsync(SendMessageRequest request, string senderId);
    Task MarkAsReadAsync(Guid conversationId, string userId);
    Task<bool> IsParticipantAsync(Guid conversationId, string userId);
}
