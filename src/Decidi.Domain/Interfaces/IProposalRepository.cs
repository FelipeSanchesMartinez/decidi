using Decidi.Domain.Entities;

namespace Decidi.Domain.Interfaces;

public interface IProposalRepository : IRepository<Proposal>
{
    Task<IEnumerable<Proposal>> GetProposalsByProjectIdAsync(Guid projectId);
    Task<IEnumerable<Proposal>> GetProposalsByFreelancerIdAsync(string freelancerId);
    Task<Proposal?> GetProposalWithDetailsAsync(Guid id);
    Task<bool> HasFreelancerProposedAsync(string freelancerId, Guid projectId);
}
