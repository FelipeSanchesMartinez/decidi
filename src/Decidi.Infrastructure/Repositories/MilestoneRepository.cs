using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Decidi.Infrastructure.Repositories;

public class MilestoneRepository(AppDbContext context) : Repository<Milestone>(context), IMilestoneRepository
{
    public async Task<IEnumerable<Milestone>> GetByProjectIdAsync(Guid projectId)
    {
        return await _dbSet
            .Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.Order)
            .AsNoTracking()
            .ToListAsync();
    }
}
