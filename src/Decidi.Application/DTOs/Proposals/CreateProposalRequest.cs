using System.ComponentModel.DataAnnotations;

namespace Decidi.Application.DTOs.Proposals;

public class CreateProposalRequest
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(1, 1000000, ErrorMessage = "Valor deve ser entre R$ 1 e R$ 1.000.000")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Prazo de entrega é obrigatório")]
    [Range(1, 365, ErrorMessage = "Prazo deve ser entre 1 e 365 dias")]
    public int DeliveryDays { get; set; }

    [Required(ErrorMessage = "Carta de apresentação é obrigatória")]
    [MaxLength(3000)]
    public string CoverLetter { get; set; } = string.Empty;
}
