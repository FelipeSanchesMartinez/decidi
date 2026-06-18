using Decidi.API.Extensions;
using Decidi.Application.DTOs.Reviews;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    [Authorize(Roles = "Client")]
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> Create([FromBody] CreateReviewRequest request)
    {
        try
        {
            var userId = User.GetUserId();
            var review = await reviewService.CreateAsync(request, userId);
            return Ok(review);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Freelancer")]
    [HttpPost("from-freelancer")]
    public async Task<ActionResult<ReviewDto>> CreateFromFreelancer([FromBody] CreateFreelancerReviewRequest request)
    {
        try
        {
            var userId = User.GetUserId();
            var review = await reviewService.CreateFreelancerReviewAsync(request, userId);
            return Ok(review);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("freelancer/{freelancerId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetByFreelancer(string freelancerId)
    {
        var reviews = await reviewService.GetByFreelancerIdAsync(freelancerId);
        return Ok(reviews);
    }

    [HttpGet("client/{clientId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetByClient(string clientId)
    {
        var reviews = await reviewService.GetByClientIdAsync(clientId);
        return Ok(reviews);
    }

    /// <summary>Projetos concluídos onde o usuário ainda não avaliou o outro lado.</summary>
    [Authorize]
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<PendingReviewDto>>> GetPending()
    {
        var userId = User.GetUserId();
        var pending = await reviewService.GetPendingForUserAsync(userId);
        return Ok(pending);
    }
}
