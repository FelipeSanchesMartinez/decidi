using System.ComponentModel.DataAnnotations;

namespace Decidi.Web.Models;

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

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 1000000)]
    public decimal BudgetMin { get; set; }

    [Range(0, 1000000)]
    public decimal BudgetMax { get; set; }

    public ProjectBudgetType BudgetType { get; set; }
    public DateTime? Deadline { get; set; }

    [Required(ErrorMessage = "Categoria é obrigatória")]
    public Guid CategoryId { get; set; }

    public List<string> RequiredSkills { get; set; } = [];
}

public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}
