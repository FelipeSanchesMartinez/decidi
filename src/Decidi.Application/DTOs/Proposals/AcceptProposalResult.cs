using Decidi.Application.DTOs.Payments;

namespace Decidi.Application.DTOs.Proposals;

/// <summary>
/// Resultado de aceitar uma proposta. Sempre traz a proposta atualizada.
/// PixCharge vem preenchido quando o gateway está disponível — modo dev
/// sem chave de sandbox retorna null e o front pode pular a tela de QR.
/// </summary>
public sealed record AcceptProposalResult(
    ProposalDto Proposal,
    PixChargeDto? PixCharge);
