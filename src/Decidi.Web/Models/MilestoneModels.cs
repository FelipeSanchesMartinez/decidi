using System.ComponentModel.DataAnnotations;

namespace Decidi.Web.Models;

public class MilestoneDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public int Order { get; set; }
    public MilestoneStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid ProjectId { get; set; }
}

public class CreateMilestoneRequest
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Título é obrigatório")]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(1, 1000000)]
    public decimal Amount { get; set; }

    [Range(1, 100)]
    public int Order { get; set; }

    public DateTime? DueDate { get; set; }
}

public class UpdateMilestoneStatusRequest
{
    public MilestoneStatus NewStatus { get; set; }
}
