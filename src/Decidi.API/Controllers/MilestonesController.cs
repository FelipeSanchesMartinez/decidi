using Decidi.API.Extensions;
using Decidi.Application.DTOs.Payments;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MilestonesController(IMilestoneService milestoneService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<MilestoneDto>> Create([FromBody] CreateMilestoneRequest request)
    {
        try
        {
            var clientId = User.GetUserId();
            var milestone = await milestoneService.CreateAsync(request, clientId);
            return Created($"/api/milestones/{milestone.Id}", milestone);
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

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<MilestoneDto>>> GetByProject(Guid projectId)
    {
        var milestones = await milestoneService.GetByProjectIdAsync(projectId);
        return Ok(milestones);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<MilestoneDto>> UpdateStatus(
        Guid id, [FromBody] UpdateMilestoneStatusRequest request)
    {
        try
        {
            var userId = User.GetUserId();
            var milestone = await milestoneService.UpdateStatusAsync(id, request, userId);
            return Ok(milestone);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
