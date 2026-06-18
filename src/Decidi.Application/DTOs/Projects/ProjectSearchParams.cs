using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Projects;

public class ProjectSearchParams
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public ProjectStatus? Status { get; set; }
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public ProjectBudgetType? BudgetType { get; set; }
    public string? Skill { get; set; }
    public string? SortBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
