using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Projects;

public class ProjectListDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string DescriptionPreview { get; set; } = string.Empty;
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public ProjectBudgetType? BudgetType { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; }

    public string ClientName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public List<string> RequiredSkills { get; set; } = [];
    public int ProposalCount { get; set; }
}
