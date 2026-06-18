using System.ComponentModel.DataAnnotations;
using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Projects;

public class CreateProjectRequest
{
    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Descrição é obrigatória")]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    // Cliente NÃO informa valor (princípio do produto). Estes campos ficam opcionais
    // — só são preenchidos se o cliente quiser sinalizar uma faixa de referência.
    [Range(0, 1000000, ErrorMessage = "Orçamento mínimo inválido")]
    public decimal? BudgetMin { get; set; }

    [Range(0, 1000000, ErrorMessage = "Orçamento máximo inválido")]
    public decimal? BudgetMax { get; set; }

    public ProjectBudgetType? BudgetType { get; set; }
    public DateTime? Deadline { get; set; }

    [Required(ErrorMessage = "Categoria é obrigatória")]
    public Guid CategoryId { get; set; }

    public List<string> RequiredSkills { get; set; } = [];
}
