using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Payments;

public class UpdateMilestoneStatusRequest
{
    public MilestoneStatus NewStatus { get; set; }
}
