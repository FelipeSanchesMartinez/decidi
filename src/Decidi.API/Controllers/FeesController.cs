using Decidi.Application.DTOs.Payments;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeesController(IPlatformFeeService platformFeeService) : ControllerBase
{
    /// <summary>Taxa vigente da plataforma. Endpoint público para landing/calculadora.</summary>
    [HttpGet("current")]
    [ResponseCache(Duration = 60)]
    public async Task<ActionResult<PlatformFeeDto>> GetCurrent()
    {
        var fee = await platformFeeService.GetCurrentAsync();
        return Ok(fee);
    }
}
