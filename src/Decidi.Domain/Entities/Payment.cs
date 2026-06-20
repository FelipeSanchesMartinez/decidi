using Decidi.Domain.Common;
using Decidi.Domain.Enums;

namespace Decidi.Domain.Entities;

/// <summary>
/// Registra uma transação financeira de contratação: cobrança do cliente,
/// retenção em escrow e liberação para o profissional. GatewayRef aponta
/// para o identificador externo no provedor (ex: Asaas payment id) — usado
/// para reconciliar com webhook e ações de estorno.
/// </summary>
public class Payment : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public Guid ProposalId { get; set; }
    public Proposal Proposal { get; set; } = null!;

    public string ClientId { get; set; } = string.Empty;
    public ApplicationUser Client { get; set; } = null!;

    public string FreelancerId { get; set; } = string.Empty;
    public ApplicationUser Freelancer { get; set; } = null!;

    /// <summary>Valor bruto da proposta aceita.</summary>
    public decimal GrossAmount { get; set; }

    /// <summary>Taxa fixa cobrada do cliente (snapshot da PlatformFee vigente).</summary>
    public decimal ClientFee { get; set; }

    /// <summary>Taxa fixa cobrada do profissional (snapshot).</summary>
    public decimal FreelancerFee { get; set; }

    /// <summary>Comissão percentual aplicada sobre o GrossAmount (snapshot).</summary>
    public decimal CommissionPct { get; set; }

    /// <summary>Comissão calculada em valor absoluto.</summary>
    public decimal CommissionAmount { get; set; }

    /// <summary>Valor líquido a ser pago ao profissional após taxas.</summary>
    public decimal NetToFreelancer { get; set; }

    /// <summary>Valor total recebido da plataforma (ClientFee + CommissionAmount + FreelancerFee).</summary>
    public decimal PlatformRevenue { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    /// <summary>Referência opcional ao identificador da transação no gateway.</summary>
    public string? GatewayRef { get; set; }

    /// <summary>FK opcional para a PlatformFee usada como base — permite auditoria histórica.</summary>
    public Guid? PlatformFeeId { get; set; }
    public PlatformFee? PlatformFee { get; set; }

    public DateTime? EscrowedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
}
