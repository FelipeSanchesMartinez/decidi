using Decidi.Domain.Common;

namespace Decidi.Domain.Entities;

/// <summary>
/// Tabela versionada de taxas da plataforma. Permite mudar valores sem deploy
/// e manter histórico de transações com a taxa vigente na época.
/// </summary>
public class PlatformFee : BaseEntity
{
    /// <summary>Data a partir da qual esta versão entra em vigor (UTC).</summary>
    public DateTime EffectiveFrom { get; set; }

    /// <summary>Taxa fixa cobrada do cliente ao aceitar uma proposta (BRL).</summary>
    public decimal ClientFee { get; set; }

    /// <summary>Taxa fixa cobrada do profissional ao receber pagamento (BRL).</summary>
    public decimal FreelancerFee { get; set; }

    /// <summary>Percentual de comissão sobre o valor recebido pelo profissional (0–100).</summary>
    public decimal CommissionPct { get; set; }

    /// <summary>Apenas uma linha pode estar ativa por vez.</summary>
    public bool IsActive { get; set; }

    public string? Note { get; set; }
}
