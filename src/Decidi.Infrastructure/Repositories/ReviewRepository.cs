using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Decidi.Infrastructure.Repositories;

public class ReviewRepository(AppDbContext context) : Repository<Review>(context), IReviewRepository
{
    public async Task<IEnumerable<Review>> GetByFreelancerIdAsync(string freelancerId, bool onlyReleased = true)
    {
        // Reviews recebidas pelo freelancer = aquelas em que ele é o avaliado (FreelancerId = ele)
        // e quem avaliou foi o Client.
        var q = _dbSet
            .Where(r => r.FreelancerId == freelancerId && r.ReviewerRole == ReviewerRole.Client);
        if (onlyReleased) q = q.Where(r => r.Visibility == ReviewVisibility.Released);

        return await q
            .Include(r => r.Client)
            .Include(r => r.Project)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByClientIdAsync(string clientId, bool onlyReleased = true)
    {
        // Reviews recebidas pelo cliente = aquelas em que ele é avaliado (ClientId = ele) e
        // quem avaliou foi o Freelancer.
        var q = _dbSet
            .Where(r => r.ClientId == clientId && r.ReviewerRole == ReviewerRole.Freelancer);
        if (onlyReleased) q = q.Where(r => r.Visibility == ReviewVisibility.Released);

        return await q
            .Include(r => r.Freelancer)
            .Include(r => r.Project)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Review?> GetByProjectAndReviewerAsync(Guid projectId, ReviewerRole reviewerRole)
    {
        return await _dbSet.FirstOrDefaultAsync(r =>
            r.ProjectId == projectId && r.ReviewerRole == reviewerRole);
    }

    public async Task<double> GetAverageRatingAsync(string userId, ReviewerRole reviewerRole, bool onlyReleased = true)
    {
        // reviewerRole = papel de QUEM AVALIOU. Filtra o lado "recebido".
        // Se userId é um Freelancer, queremos reviews onde ReviewerRole=Client e FreelancerId=userId.
        // Se userId é um Client, queremos reviews onde ReviewerRole=Freelancer e ClientId=userId.
        IQueryable<Review> q = _dbSet.Where(r => r.ReviewerRole == reviewerRole);
        q = reviewerRole == ReviewerRole.Client
            ? q.Where(r => r.FreelancerId == userId)
            : q.Where(r => r.ClientId == userId);
        if (onlyReleased) q = q.Where(r => r.Visibility == ReviewVisibility.Released);

        // Average em memória — volume pequeno por usuário.
        var ratings = await q.Select(r => r.Rating).ToListAsync();
        return ratings.Count != 0 ? ratings.Average() : 0;
    }

    public async Task<int> GetCountAsync(string userId, ReviewerRole reviewerRole, bool onlyReleased = true)
    {
        IQueryable<Review> q = _dbSet.Where(r => r.ReviewerRole == reviewerRole);
        q = reviewerRole == ReviewerRole.Client
            ? q.Where(r => r.FreelancerId == userId)
            : q.Where(r => r.ClientId == userId);
        if (onlyReleased) q = q.Where(r => r.Visibility == ReviewVisibility.Released);

        return await q.CountAsync();
    }

    public void UpdateRange(IEnumerable<Review> reviews)
    {
        _dbSet.UpdateRange(reviews);
    }

    public async Task<IEnumerable<Review>> GetPendingExpiredForUserAsync(string userId, DateTime threshold)
    {
        // Usa os índices (FreelancerId, Visibility) e (ClientId, Visibility) criados na Sprint 5.
        return await _dbSet
            .Where(r => (r.FreelancerId == userId || r.ClientId == userId)
                && r.Visibility == ReviewVisibility.Pending
                && r.CreatedAt <= threshold)
            .ToListAsync();
    }

    public async Task<IEnumerable<Review>> GetByProjectsAndReviewerAsync(IEnumerable<Guid> projectIds, ReviewerRole reviewerRole)
    {
        var ids = projectIds.ToList();
        if (ids.Count == 0) return [];
        return await _dbSet
            .Where(r => ids.Contains(r.ProjectId) && r.ReviewerRole == reviewerRole)
            .AsNoTracking()
            .ToListAsync();
    }
}
