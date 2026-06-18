using Decidi.Application.DTOs.Proposals;
using Decidi.Domain.Entities;

namespace Decidi.Application.Mappings;

public static class ProposalMappings
{
    public static ProposalDto ToDto(this Proposal proposal) => new()
    {
        Id = proposal.Id,
        Amount = proposal.Amount,
        DeliveryDays = proposal.DeliveryDays,
        CoverLetter = proposal.CoverLetter,
        Status = proposal.Status,
        CreatedAt = proposal.CreatedAt,
        FreelancerId = proposal.FreelancerId,
        FreelancerName = proposal.Freelancer?.FullName ?? string.Empty,
        FreelancerTitle = proposal.Freelancer?.FreelancerProfile?.Title,
        FreelancerAvatarUrl = proposal.Freelancer?.AvatarUrl,
        ProjectId = proposal.ProjectId,
        ProjectTitle = proposal.Project?.Title ?? string.Empty
    };

    public static Proposal ToEntity(this CreateProposalRequest request, string freelancerId) => new()
    {
        Amount = request.Amount,
        DeliveryDays = request.DeliveryDays,
        CoverLetter = request.CoverLetter,
        ProjectId = request.ProjectId,
        FreelancerId = freelancerId
    };
}
