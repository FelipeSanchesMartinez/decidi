using Decidi.Application.DTOs.Payments;
using Decidi.Domain.Entities;

namespace Decidi.Application.Mappings;

public static class MilestoneMappings
{
    public static MilestoneDto ToDto(this Milestone milestone) => new()
    {
        Id = milestone.Id,
        Title = milestone.Title,
        Description = milestone.Description,
        Amount = milestone.Amount,
        Order = milestone.Order,
        Status = milestone.Status,
        DueDate = milestone.DueDate,
        PaidAt = milestone.PaidAt,
        CreatedAt = milestone.CreatedAt,
        ProjectId = milestone.ProjectId
    };

    public static Milestone ToEntity(this CreateMilestoneRequest request) => new()
    {
        Title = request.Title,
        Description = request.Description,
        Amount = request.Amount,
        Order = request.Order,
        DueDate = request.DueDate,
        ProjectId = request.ProjectId
    };
}
