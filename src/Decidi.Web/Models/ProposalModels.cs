using System.ComponentModel.DataAnnotations;

namespace Decidi.Web.Models;

public class CreateProposalRequest
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Valor é obrigatório")]
    [Range(1, 1000000)]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Prazo é obrigatório")]
    [Range(1, 365)]
    public int DeliveryDays { get; set; }

    [Required(ErrorMessage = "Carta de apresentação é obrigatória")]
    [MaxLength(3000)]
    public string CoverLetter { get; set; } = string.Empty;
}

public class ProposalDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public int DeliveryDays { get; set; }
    public string CoverLetter { get; set; } = string.Empty;
    public ProposalStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string FreelancerId { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public string? FreelancerTitle { get; set; }
    public string? FreelancerAvatarUrl { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
}

/// <summary>
/// Espelho do AcceptProposalResult do backend. PixCharge vem null quando o
/// gateway está em modo dev (sem chave) — o front mostra só a confirmação.
/// </summary>
public class AcceptProposalResult
{
    public ProposalDto Proposal { get; set; } = new();
    public PixChargeDto? PixCharge { get; set; }
}

public class PixChargeDto
{
    public string GatewayRef { get; set; } = string.Empty;
    public string QrCodeBase64 { get; set; } = string.Empty;
    public string CopyPaste { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime ExpiresAt { get; set; }
}
