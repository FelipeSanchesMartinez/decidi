using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Decidi.Infrastructure.Repositories;

public class SkillRepository(AppDbContext context) : Repository<Skill>(context), ISkillRepository
{
    public async Task<IEnumerable<Skill>> GetAllGroupedAsync()
    {
        return await _dbSet.OrderBy(s => s.Group).ThenBy(s => s.Name).AsNoTracking().ToListAsync();
    }

    public async Task<IEnumerable<Skill>> GetByNamesAsync(IEnumerable<string> names)
    {
        var nameList = names.ToList();
        return await _dbSet.Where(s => nameList.Contains(s.Name)).ToListAsync();
    }
}
