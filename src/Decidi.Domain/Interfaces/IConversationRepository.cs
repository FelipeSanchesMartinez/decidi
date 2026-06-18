using Decidi.Domain.Entities;

namespace Decidi.Domain.Interfaces;

public interface IConversationRepository : IRepository<Conversation>
{
    Task<Conversation?> GetConversationWithMessagesAsync(Guid conversationId, int messageCount = 50);
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(string userId);
    Task<Conversation?> GetByProjectAndUsersAsync(Guid projectId, string clientId, string freelancerId);
    Task<Conversation?> GetWithProjectAsync(Guid conversationId);
}
