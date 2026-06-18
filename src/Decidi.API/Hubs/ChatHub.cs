using Decidi.API.Extensions;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Decidi.API.Hubs;

[Authorize]
public class ChatHub(IChatService chatService) : Hub
{
    public async Task JoinConversation(string conversationId)
    {
        if (!Guid.TryParse(conversationId, out var convId))
            throw new HubException("Identificador de conversa inválido.");

        var userId = Context.User?.GetUserId()
            ?? throw new HubException("Usuário não autenticado.");

        if (!await chatService.IsParticipantAsync(convId, userId))
            throw new HubException("Você não tem acesso a esta conversa.");

        await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{convId}");
    }

    public async Task LeaveConversation(string conversationId)
    {
        if (!Guid.TryParse(conversationId, out var convId)) return;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{convId}");
    }
}
