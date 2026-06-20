using Decidi.Domain.Payments;

namespace Decidi.Application.Interfaces;

/// <summary>
/// Processa um evento normalizado vindo do gateway de pagamento.
/// Implementado pelo Application; chamado pelo WebhooksController da API.
/// Idempotência é responsabilidade do handler.
/// </summary>
public interface IPaymentEventHandler
{
    Task<PaymentEventOutcome> HandleAsync(GatewayEvent evt, CancellationToken ct = default);
}

public enum PaymentEventOutcome
{
    /// <summary>Aplicado: o estado do Payment mudou.</summary>
    Applied,
    /// <summary>Ignorado: evento não muda o estado atual (status já está lá).</summary>
    NoChange,
    /// <summary>Charge não encontrado — pode ser webhook fora do nosso domínio.</summary>
    Unknown
}
