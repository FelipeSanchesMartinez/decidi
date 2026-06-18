using Decidi.Domain.Common;
using Decidi.Domain.Enums;

namespace Decidi.Domain.Entities;

public class Proposal : BaseEntity
{
    public decimal Amount { get; set; }
    public int DeliveryDays { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; } = ProposalStatus.Pending;

    public string FreelancerId { get; set; } = string.Empty;
    public ApplicationUser Freelancer { get; set; } = null!;

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    /// <summary>Concurrency token — atualizado a cada SaveChanges para detectar update concorrente.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
