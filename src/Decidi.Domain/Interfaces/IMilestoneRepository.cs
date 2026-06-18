using Decidi.Domain.Entities;

namespace Decidi.Domain.Interfaces;

public interface IMilestoneRepository : IRepository<Milestone>
{
    Task<IEnumerable<Milestone>> GetByProjectIdAsync(Guid projectId);
}
