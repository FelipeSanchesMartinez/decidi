using Decidi.Application.DTOs.Projects;
using Decidi.Domain.Entities;

namespace Decidi.Application.Mappings;

public static class ProjectMappings
{
    public static ProjectDto ToDto(this Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        Description = project.Description,
        BudgetMin = project.BudgetMin,
        BudgetMax = project.BudgetMax,
        BudgetType = project.BudgetType,
        Status = project.Status,
        Deadline = project.Deadline,
        CreatedAt = project.CreatedAt,
        ClientId = project.ClientId,
        ClientName = project.Client?.FullName ?? string.Empty,
        CategoryId = project.CategoryId,
        CategoryName = project.Category?.Name ?? string.Empty,
        AcceptedFreelancerId = project.AcceptedFreelancerId,
        AcceptedFreelancerName = project.AcceptedFreelancer?.FullName,
        RequiredSkills = project.RequiredSkills.Select(s => s.Name).ToList(),
        ProposalCount = project.Proposals.Count
    };

    public static ProjectListDto ToListDto(this Project project) => new()
    {
        Id = project.Id,
        Title = project.Title,
        DescriptionPreview = project.Description.Length > 200
            ? project.Description[..200] + "..."
            : project.Description,
        BudgetMin = project.BudgetMin,
        BudgetMax = project.BudgetMax,
        BudgetType = project.BudgetType,
        Status = project.Status,
        Deadline = project.Deadline,
        CreatedAt = project.CreatedAt,
        ClientName = project.Client?.FullName ?? string.Empty,
        CategoryName = project.Category?.Name ?? string.Empty,
        RequiredSkills = project.RequiredSkills.Select(s => s.Name).ToList(),
        ProposalCount = project.Proposals.Count
    };

    public static Project ToEntity(this CreateProjectRequest request, string clientId) => new()
    {
        Title = request.Title,
        Description = request.Description,
        BudgetMin = request.BudgetMin,
        BudgetMax = request.BudgetMax,
        BudgetType = request.BudgetType,
        Deadline = request.Deadline,
        CategoryId = request.CategoryId,
        ClientId = clientId
    };
}
