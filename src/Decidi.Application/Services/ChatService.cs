using Decidi.Application.DTOs.Chat;
using Decidi.Application.Interfaces;
using Decidi.Application.Mappings;
using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Decidi.Domain.Interfaces;

namespace Decidi.Application.Services;

public class ChatService(
    IConversationRepository conversationRepository,
    IProjectRepository projectRepository,
    IProposalRepository proposalRepository,
    IRepository<Message> messageRepository,
    INotificationService notificationService,
    ISanitizer sanitizer,
    IContactGuard contactGuard,
    IUnitOfWork unitOfWork) : IChatService
{
    // Status onde já existe contratação: liberar troca de contatos.
    private static bool ContactsAreAllowed(ProjectStatus status) =>
        status == ProjectStatus.Contracted ||
        status == ProjectStatus.InProgress ||
        status == ProjectStatus.Completed;

    public async Task<ConversationDto> GetOrCreateConversationAsync(
        Guid projectId, string freelancerId, string currentUserId)
    {
        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        // Identidade do cliente vem do projeto (não pode ser informada pelo chamador).
        var clientId = project.ClientId;

        // Caller precisa ser o cliente do projeto OU o próprio freelancer da rota.
        if (currentUserId != clientId && currentUserId != freelancerId)
            throw new UnauthorizedAccessException("Você não pode iniciar esta conversa.");

        // Freelancer só pode iniciar conversa se tiver proposta no projeto.
        if (currentUserId == freelancerId &&
            !await proposalRepository.HasFreelancerProposedAsync(freelancerId, projectId))
            throw new UnauthorizedAccessException("Envie uma proposta antes de iniciar conversa.");

        var conversation = await conversationRepository
            .GetByProjectAndUsersAsync(projectId, clientId, freelancerId);

        if (conversation is null)
        {
            conversation = new Conversation
            {
                ProjectId = projectId,
                ClientId = clientId,
                FreelancerId = freelancerId
            };
            await conversationRepository.AddAsync(conversation);
            await unitOfWork.SaveChangesAsync();

            conversation = await conversationRepository
                .GetConversationWithMessagesAsync(conversation.Id);
        }

        return conversation!.ToDto(currentUserId);
    }

    public async Task<IEnumerable<ConversationDto>> GetUserConversationsAsync(string userId)
    {
        var conversations = await conversationRepository.GetUserConversationsAsync(userId);
        return conversations.Select(c => c.ToDto(userId));
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, string userId)
    {
        var conversation = await conversationRepository
            .GetConversationWithMessagesAsync(conversationId)
            ?? throw new KeyNotFoundException("Conversa não encontrada.");

        if (conversation.ClientId != userId && conversation.FreelancerId != userId)
            throw new UnauthorizedAccessException("Você não tem acesso a esta conversa.");

        return conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.ToDto());
    }

    public async Task<MessageDto> SendMessageAsync(SendMessageRequest request, string senderId)
    {
        var conversation = await conversationRepository.GetWithProjectAsync(request.ConversationId)
            ?? throw new KeyNotFoundException("Conversa não encontrada.");

        if (conversation.ClientId != senderId && conversation.FreelancerId != senderId)
            throw new UnauthorizedAccessException("Você não tem acesso a esta conversa.");

        // Antifraude: só permite troca de contatos depois de contratado.
        var content = sanitizer.Sanitize(request.Content);
        var wasRedacted = false;
        if (!ContactsAreAllowed(conversation.Project.Status))
        {
            var (redacted, didRedact) = contactGuard.Redact(content);
            content = redacted;
            wasRedacted = didRedact;
        }

        var message = new Message
        {
            Content = content,
            SenderId = senderId,
            ConversationId = request.ConversationId
        };

        await messageRepository.AddAsync(message);
        await unitOfWork.SaveChangesAsync();

        // Notifica o destinatário (best-effort, com debounce de 2min anti-spam).
        try
        {
            var recipientId = conversation.ClientId == senderId
                ? conversation.FreelancerId
                : conversation.ClientId;

            if (!string.IsNullOrEmpty(recipientId))
            {
                var senderName = conversation.ClientId == senderId
                    ? (conversation.Client?.FullName ?? "Cliente")
                    : (conversation.Freelancer?.FullName ?? "Profissional");

                var preview = content.Length > 80 ? content[..80] + "…" : content;

                // Anti-spam: se já houver uma chat_message do mesmo sender em < 2min, não cria nova.
                var since = DateTime.UtcNow.AddMinutes(-2);
                var recentNotifs = await notificationService.GetUserNotificationsAsync(recipientId, take: 10);
                var alreadyNotified = recentNotifs.Any(n =>
                    n.Type == "chat_message"
                    && n.CreatedAt >= since
                    && n.Link == $"/chat/{conversation.Id}");

                if (!alreadyNotified)
                {
                    await notificationService.CreateAsync(
                        recipientId, "chat_message",
                        $"Nova mensagem de {senderName}",
                        preview,
                        $"/chat/{conversation.Id}");
                }
            }
        }
        catch { /* notification é best-effort */ }

        return new MessageDto
        {
            Id = message.Id,
            Content = message.Content,
            SenderId = message.SenderId,
            SenderName = string.Empty,
            IsRead = false,
            CreatedAt = message.CreatedAt,
            WasRedacted = wasRedacted
        };
    }

    public async Task MarkAsReadAsync(Guid conversationId, string userId)
    {
        var conversation = await conversationRepository
            .GetConversationWithMessagesAsync(conversationId)
            ?? throw new KeyNotFoundException("Conversa não encontrada.");

        if (conversation.ClientId != userId && conversation.FreelancerId != userId)
            throw new UnauthorizedAccessException("Você não tem acesso a esta conversa.");

        foreach (var message in conversation.Messages.Where(m => !m.IsRead && m.SenderId != userId))
        {
            message.IsRead = true;
        }

        await unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> IsParticipantAsync(Guid conversationId, string userId)
    {
        var conversation = await conversationRepository.GetByIdAsync(conversationId);
        if (conversation is null) return false;
        return conversation.ClientId == userId || conversation.FreelancerId == userId;
    }
}
