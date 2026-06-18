using Decidi.Domain.Entities;

namespace Decidi.Domain.Interfaces;

public interface ISkillRepository : IRepository<Skill>
{
    Task<IEnumerable<Skill>> GetAllGroupedAsync();
    Task<IEnumerable<Skill>> GetByNamesAsync(IEnumerable<string> names);
}
