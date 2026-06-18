using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Proposals;

public class ProposalDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public int DeliveryDays { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public string FreelancerId { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public string? FreelancerTitle { get; set; }
    public string? FreelancerAvatarUrl { get; set; }

    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
}
