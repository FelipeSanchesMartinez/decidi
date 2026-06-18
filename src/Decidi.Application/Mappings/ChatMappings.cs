using Decidi.Application.DTOs.Chat;
using Decidi.Domain.Entities;

namespace Decidi.Application.Mappings;

public static class ChatMappings
{
    public static ConversationDto ToDto(this Conversation conversation, string currentUserId)
    {
        var isClient = conversation.ClientId == currentUserId;
        var otherUser = isClient ? conversation.Freelancer : conversation.Client;
        var lastMessage = conversation.Messages
            .OrderByDescending(m => m.CreatedAt)
            .FirstOrDefault();

        return new ConversationDto
        {
            Id = conversation.Id,
            ProjectId = conversation.ProjectId,
            ProjectTitle = conversation.Project?.Title ?? string.Empty,
            OtherUserId = otherUser?.Id ?? string.Empty,
            OtherUserName = otherUser?.FullName ?? string.Empty,
            OtherUserAvatarUrl = otherUser?.AvatarUrl,
            LastMessage = lastMessage?.Content,
            LastMessageAt = lastMessage?.CreatedAt,
            UnreadCount = conversation.Messages.Count(m => !m.IsRead && m.SenderId != currentUserId)
        };
    }

    public static MessageDto ToDto(this Message message) => new()
    {
        Id = message.Id,
        Content = message.Content,
        SenderId = message.SenderId,
        SenderName = message.Sender?.FullName ?? string.Empty,
        IsRead = message.IsRead,
        CreatedAt = message.CreatedAt
    };
}
