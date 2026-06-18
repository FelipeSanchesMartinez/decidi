using Decidi.API.Extensions;
using Decidi.API.Hubs;
using Decidi.Application.DTOs.Chat;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatController(
    IChatService chatService,
    IHubContext<ChatHub> hubContext) : ControllerBase
{
    [HttpGet("conversations")]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations()
    {
        var userId = User.GetUserId();
        var conversations = await chatService.GetUserConversationsAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("conversations/{id:guid}/messages")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
            var messages = await chatService.GetMessagesAsync(id, userId);
            return Ok(messages);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPost("conversations/{projectId:guid}/{freelancerId}")]
    public async Task<ActionResult<ConversationDto>> CreateConversation(Guid projectId, string freelancerId)
    {
        try
        {
            var userId = User.GetUserId();
            var conversation = await chatService.GetOrCreateConversationAsync(
                projectId, freelancerId, userId);
            return Ok(conversation);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
    }

    [HttpPut("conversations/{id:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid id)
    {
        try
        {
            var userId = User.GetUserId();
            await chatService.MarkAsReadAsync(id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("messages")]
    public async Task<ActionResult<MessageDto>> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var senderId = User.GetUserId();
            var message = await chatService.SendMessageAsync(request, senderId);

            await hubContext.Clients
                .Group($"conversation_{request.ConversationId}")
                .SendAsync("ReceiveMessage", message);

            return Ok(message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }
}
