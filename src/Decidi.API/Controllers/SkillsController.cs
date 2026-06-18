using Decidi.Application.DTOs.Common;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SkillsController(ISkillService skillService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SkillDto>>> GetAll()
    {
        var skills = await skillService.GetAllAsync();
        return Ok(skills);
    }
}
