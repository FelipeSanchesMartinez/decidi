using Decidi.API.Extensions;
using Decidi.Application.DTOs.Notifications;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/notification-preferences")]
[Authorize]
public class NotificationPreferencesController(INotificationPreferencesService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationPreferencesDto>> Get()
    {
        var dto = await service.GetForUserAsync(User.GetUserId());
        return Ok(dto);
    }

    [HttpPut]
    public async Task<ActionResult<NotificationPreferencesDto>> Update([FromBody] NotificationPreferencesDto prefs)
    {
        var dto = await service.UpdateForUserAsync(User.GetUserId(), prefs);
        return Ok(dto);
    }
}
