using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Decidi.Infrastructure.Repositories;

public class ProposalRepository(AppDbContext context) : Repository<Proposal>(context), IProposalRepository
{
    public async Task<IEnumerable<Proposal>> GetProposalsByProjectIdAsync(Guid projectId)
    {
        return await _dbSet
            .Where(p => p.ProjectId == projectId)
            .Include(p => p.Freelancer)
                .ThenInclude(f => f.FreelancerProfile)
            .Include(p => p.Project)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Proposal>> GetProposalsByFreelancerIdAsync(string freelancerId)
    {
        return await _dbSet
            .Where(p => p.FreelancerId == freelancerId)
            .Include(p => p.Freelancer)
                .ThenInclude(f => f.FreelancerProfile)
            .Include(p => p.Project)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Proposal?> GetProposalWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Freelancer)
                .ThenInclude(f => f.FreelancerProfile)
            .Include(p => p.Project)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<bool> HasFreelancerProposedAsync(string freelancerId, Guid projectId)
    {
        return await _dbSet.AnyAsync(p => p.FreelancerId == freelancerId && p.ProjectId == projectId);
    }
}
