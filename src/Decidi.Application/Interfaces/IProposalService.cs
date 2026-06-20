using Decidi.Application.DTOs.Proposals;

namespace Decidi.Application.Interfaces;

public interface IProposalService
{
    Task<ProposalDto> CreateAsync(CreateProposalRequest request, string freelancerId);
    Task<IEnumerable<ProposalDto>> GetByProjectIdAsync(Guid projectId, string viewerId);
    Task<IEnumerable<ProposalDto>> GetByFreelancerIdAsync(string freelancerId);
    Task<AcceptProposalResult> AcceptAsync(Guid proposalId, string clientId);
    Task<ProposalDto> RejectAsync(Guid proposalId, string clientId);
    Task WithdrawAsync(Guid proposalId, string freelancerId);
}
