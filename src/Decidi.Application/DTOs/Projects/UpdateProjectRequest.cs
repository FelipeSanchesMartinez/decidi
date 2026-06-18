using System.ComponentModel.DataAnnotations;
using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Projects;

public class UpdateProjectRequest
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    [Range(0, 1000000)]
    public decimal? BudgetMin { get; set; }

    [Range(0, 1000000)]
    public decimal? BudgetMax { get; set; }

    public ProjectBudgetType? BudgetType { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid CategoryId { get; set; }
    public List<string> RequiredSkills { get; set; } = [];
}
