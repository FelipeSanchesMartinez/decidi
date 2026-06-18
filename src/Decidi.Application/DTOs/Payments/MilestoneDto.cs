using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Payments;

public class MilestoneDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public int Order { get; set; }
    public MilestoneStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid ProjectId { get; set; }
}
