using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Projects;

public class ProjectDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public ProjectBudgetType? BudgetType { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }

    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? AcceptedFreelancerId { get; set; }
    public string? AcceptedFreelancerName { get; set; }

    public List<string> RequiredSkills { get; set; } = [];
    public int ProposalCount { get; set; }
}
