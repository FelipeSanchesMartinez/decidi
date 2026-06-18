using Decidi.Domain.Common;
using Decidi.Domain.Enums;

namespace Decidi.Domain.Entities;

public class Milestone : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public int Order { get; set; }
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
