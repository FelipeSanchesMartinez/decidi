using Decidi.API.Extensions;
using Decidi.Application.DTOs.Common;
using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Decidi.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatsController(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    IMemoryCache cache) : ControllerBase
{
    private const string PublicCacheKey = "stats:public";
    /// <summary>Time zone do Brasil — tolerante a nome Windows vs IANA.</summary>
    private static TimeZoneInfo BrazilTimeZone()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); }
        catch (TimeZoneNotFoundException) { return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"); }
    }

    /// <summary>
    /// Contadores agregados para uso público (landing, hero). Sem dados pessoais.
    /// </summary>
    [HttpGet("public")]
    [ResponseCache(Duration = 300)]
    public async Task<ActionResult<PublicStatsDto>> GetPublic()
    {
        // Cache server-side de 5 min — evita 4 COUNTs no banco por request anonimo
        // mesmo sem ResponseCachingMiddleware configurado.
        if (cache.TryGetValue<PublicStatsDto>(PublicCacheKey, out var cached) && cached is not null)
            return Ok(cached);

        var totalProjects = await db.Projects.CountAsync();
        var completed = await db.Projects.CountAsync(p => p.Status == ProjectStatus.Completed);
        var categories = await db.Categories.CountAsync();
        var activeFreelancers = await userManager.Users.CountAsync(u => u.Role == UserRole.Freelancer);

        var dto = new PublicStatsDto
        {
            TotalProjects = totalProjects,
            CompletedProjects = completed,
            Categories = categories,
            ActiveFreelancers = activeFreelancers
        };
        cache.Set(PublicCacheKey, dto, TimeSpan.FromMinutes(5));
        return Ok(dto);
    }

    /// <summary>KPIs do cliente autenticado para o dashboard.</summary>
    [Authorize(Roles = "Client")]
    [HttpGet("client")]
    public async Task<ActionResult<ClientStatsDto>> GetClient()
    {
        var userId = User.GetUserId();

        var activeStatuses = new[]
        {
            ProjectStatus.ReceivingProposals, ProjectStatus.InNegotiation,
            ProjectStatus.Contracted, ProjectStatus.InProgress
        };

        var clientProjectIds = await db.Projects
            .Where(p => p.ClientId == userId)
            .Select(p => p.Id)
            .ToListAsync();

        var activeProjects = await db.Projects
            .CountAsync(p => p.ClientId == userId && activeStatuses.Contains(p.Status));

        var proposalsToReview = await db.Proposals
            .CountAsync(pr => clientProjectIds.Contains(pr.ProjectId)
                && pr.Status == ProposalStatus.Pending);

        var conversations = await db.Conversations.CountAsync(c => c.ClientId == userId);

        // Whitelist explícita: só conta gasto efetivo (em escrow ou liberado).
        // Refunded/Disputed/Cancelled/Pending NÃO entram aqui.
        var totalSpent = await db.Payments
            .Where(p => p.ClientId == userId
                && (p.Status == PaymentStatus.Escrowed || p.Status == PaymentStatus.Released))
            .SumAsync(p => (decimal?)p.GrossAmount) ?? 0m;

        return Ok(new ClientStatsDto
        {
            ActiveProjects = activeProjects,
            ProposalsToReview = proposalsToReview,
            Conversations = conversations,
            TotalSpentApprox = totalSpent
        });
    }

    /// <summary>KPIs do freelancer autenticado para o dashboard.</summary>
    [Authorize(Roles = "Freelancer")]
    [HttpGet("freelancer")]
    public async Task<ActionResult<FreelancerStatsDto>> GetFreelancer()
    {
        var userId = User.GetUserId();

        // Mes corrente do ponto de vista do usuario brasileiro (BRT/UTC-3), convertido de volta para UTC.
        var nowBrt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BrazilTimeZone());
        var monthStartBrt = new DateTime(nowBrt.Year, nowBrt.Month, 1, 0, 0, 0, DateTimeKind.Unspecified);
        var monthStart = TimeZoneInfo.ConvertTimeToUtc(monthStartBrt, BrazilTimeZone());

        var earnings = await db.Payments
            .Where(p => p.FreelancerId == userId
                && p.Status == PaymentStatus.Released
                && p.ReleasedAt != null && p.ReleasedAt >= monthStart)
            .SumAsync(p => (decimal?)p.NetToFreelancer) ?? 0m;

        var totalProposalsConsidered = await db.Proposals
            .CountAsync(p => p.FreelancerId == userId && p.Status != ProposalStatus.Withdrawn);

        var acceptedProposals = await db.Proposals
            .CountAsync(p => p.FreelancerId == userId && p.Status == ProposalStatus.Accepted);

        var acceptanceRate = totalProposalsConsidered > 0
            ? (double)acceptedProposals / totalProposalsConsidered
            : 0;

        var pendingProposals = await db.Proposals
            .CountAsync(p => p.FreelancerId == userId && p.Status == ProposalStatus.Pending);

        var activeContracts = await db.Projects
            .CountAsync(p => p.AcceptedFreelancerId == userId
                && (p.Status == ProjectStatus.Contracted || p.Status == ProjectStatus.InProgress));

        // Reviews recebidas como freelancer (quem avaliou foi Client) e já liberadas.
        var ratings = await db.Reviews
            .Where(r => r.FreelancerId == userId
                && r.ReviewerRole == ReviewerRole.Client
                && r.Visibility == ReviewVisibility.Released)
            .Select(r => r.Rating)
            .ToListAsync();

        return Ok(new FreelancerStatsDto
        {
            EarningsThisMonth = earnings,
            AcceptanceRate = acceptanceRate,
            PendingProposals = pendingProposals,
            ActiveContracts = activeContracts,
            AverageRating = ratings.Count > 0 ? ratings.Average() : 0,
            TotalReviews = ratings.Count
        });
    }
}
