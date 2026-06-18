using Decidi.Application.DTOs.Payments;

namespace Decidi.Application.Interfaces;

public interface IMilestoneService
{
    Task<MilestoneDto> CreateAsync(CreateMilestoneRequest request, string clientId);
    Task<IEnumerable<MilestoneDto>> GetByProjectIdAsync(Guid projectId);
    Task<MilestoneDto> UpdateStatusAsync(Guid milestoneId, UpdateMilestoneStatusRequest request, string userId);
}
