using Decidi.API.Extensions;
using Decidi.Application.DTOs.Common;
using Decidi.Application.DTOs.Projects;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController(IProjectService projectService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProjectListDto>>> Search([FromQuery] ProjectSearchParams searchParams)
    {
        var result = await projectService.SearchAsync(searchParams);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> GetById(Guid id)
    {
        try
        {
            var project = await projectService.GetByIdAsync(id);
            return Ok(project);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost]
    public async Task<ActionResult<ProjectDto>> Create([FromBody] CreateProjectRequest request)
    {
        try
        {
            var clientId = User.GetUserId();
            var project = await projectService.CreateAsync(request, clientId);
            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProjectDto>> Update(Guid id, [FromBody] UpdateProjectRequest request)
    {
        try
        {
            var clientId = User.GetUserId();
            var project = await projectService.UpdateAsync(id, request, clientId);
            return Ok(project);
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

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var clientId = User.GetUserId();
            await projectService.DeleteAsync(id, clientId);
            return NoContent();
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

    [Authorize]
    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<ProjectListDto>>> GetMyProjects()
    {
        var clientId = User.GetUserId();
        var projects = await projectService.GetByClientIdAsync(clientId);
        return Ok(projects);
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<ProjectDto>> Start(Guid id)
    {
        try
        {
            var project = await projectService.StartExecutionAsync(id, User.GetUserId());
            return Ok(project);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ProjectDto>> Complete(Guid id)
    {
        try
        {
            var project = await projectService.CompleteAsync(id, User.GetUserId());
            return Ok(project);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    public record CancelProjectRequest(string? Reason);

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ProjectDto>> Cancel(Guid id, [FromBody] CancelProjectRequest? body)
    {
        try
        {
            var project = await projectService.CancelAsync(id, User.GetUserId(), body?.Reason);
            return Ok(project);
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }
}
