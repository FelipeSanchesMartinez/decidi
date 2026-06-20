using Decidi.API.Extensions;
using Decidi.Application.DTOs.Proposals;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProposalsController(IProposalService proposalService) : ControllerBase
{
    [Authorize(Policy = "EmailConfirmed")]
    [HttpPost]
    public async Task<ActionResult<ProposalDto>> Create([FromBody] CreateProposalRequest request)
    {
        try
        {
            var freelancerId = User.GetUserId();
            var proposal = await proposalService.CreateAsync(request, freelancerId);
            return Created($"/api/proposals/{proposal.Id}", proposal);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<ProposalDto>>> GetByProject(Guid projectId)
    {
        try
        {
            var viewerId = User.GetUserId();
            var proposals = await proposalService.GetByProjectIdAsync(projectId, viewerId);
            return Ok(proposals);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("my")]
    public async Task<ActionResult<IEnumerable<ProposalDto>>> GetMyProposals()
    {
        var freelancerId = User.GetUserId();
        var proposals = await proposalService.GetByFreelancerIdAsync(freelancerId);
        return Ok(proposals);
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPut("{id:guid}/accept")]
    public async Task<ActionResult<AcceptProposalResult>> Accept(Guid id)
    {
        try
        {
            var clientId = User.GetUserId();
            var result = await proposalService.AcceptAsync(id, clientId);
            return Ok(result);
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
        catch (Decidi.Domain.Payments.PaymentGatewayException ex)
        {
            // Gateway fora do ar / chave inválida — tx já foi revertida, então
            // a proposta NÃO foi aceita. Mostra erro técnico mais claro.
            return StatusCode(502, new { message = "Falha ao gerar cobrança PIX: " + ex.Message });
        }
    }

    [Authorize(Policy = "EmailConfirmed")]
    [HttpPut("{id:guid}/reject")]
    public async Task<ActionResult<ProposalDto>> Reject(Guid id)
    {
        try
        {
            var clientId = User.GetUserId();
            var proposal = await proposalService.RejectAsync(id, clientId);
            return Ok(proposal);
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

    [HttpPut("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id)
    {
        try
        {
            var freelancerId = User.GetUserId();
            await proposalService.WithdrawAsync(id, freelancerId);
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
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
