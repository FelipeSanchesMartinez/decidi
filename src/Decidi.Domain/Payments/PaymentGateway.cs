namespace Decidi.Domain.Payments;

/// <summary>
/// Abstração sobre o gateway de pagamento. Permite trocar de provedor
/// (Asaas, Mercado Pago, Stripe) sem tocar nas regras de negócio do
/// Application/Domain. A implementação concreta vive no Infrastructure.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Nome do provedor — útil para log e métricas.</summary>
    string ProviderName { get; }

    /// <summary>
    /// Cria uma cobrança PIX para o cliente. O resultado traz QR Code,
    /// copia-cola e o identificador externo (GatewayRef) que persistimos
    /// no Payment.
    /// </summary>
    Task<ChargeCreationResult> CreatePixChargeAsync(
        ChargeRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consulta o status atual da cobrança no gateway. Usado como
    /// fallback quando o webhook falha ou para reconciliar manualmente.
    /// </summary>
    Task<ChargeSnapshot> GetChargeAsync(
        string chargeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estorna integralmente uma cobrança confirmada. Disparado por
    /// disputa procedente ou cancelamento aceito.
    /// </summary>
    Task<RefundResult> RefundChargeAsync(
        string chargeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida o token compartilhado enviado pelo gateway no header do
    /// webhook. Retorna false se o segredo não bater.
    /// </summary>
    bool ValidateWebhookToken(string? receivedToken);

    /// <summary>
    /// Decodifica o payload do webhook em um evento normalizado. Devolve
    /// null se o evento não for relevante para o domínio (ex: PAYMENT_CREATED
    /// quando já sabemos do nosso próprio fluxo).
    /// </summary>
    GatewayEvent? ParseWebhookPayload(string payload);
}

/// <summary>Dados que o domínio entrega ao gateway para gerar uma cobrança.</summary>
public sealed record ChargeRequest(
    Guid PaymentId,
    decimal Amount,
    string CustomerName,
    string CustomerEmail,
    string? CustomerCpfCnpj,
    string Description,
    DateTime DueDate);

/// <summary>Resultado da criação de cobrança PIX.</summary>
public sealed record ChargeCreationResult(
    string ChargeId,
    string PixQrCodeBase64,
    string PixCopyPaste,
    DateTime PixExpiresAt,
    GatewayChargeStatus Status,
    string? CheckoutUrl);

/// <summary>Snapshot do estado da cobrança no gateway.</summary>
public sealed record ChargeSnapshot(
    string ChargeId,
    GatewayChargeStatus Status,
    decimal Amount,
    decimal NetAmount,
    DateTime? PaidAt);

/// <summary>Resultado de um pedido de estorno.</summary>
public sealed record RefundResult(
    string ChargeId,
    bool Succeeded,
    DateTime? RefundedAt,
    string? FailureReason);

/// <summary>
/// Evento normalizado do gateway, traduzido para a linguagem do domínio.
/// O Application/Service decide o que fazer com isto.
/// </summary>
public sealed record GatewayEvent(
    string EventId,
    string ChargeId,
    GatewayChargeStatus NewStatus,
    decimal? Value,
    DateTime OccurredAt,
    string RawPayload);

/// <summary>
/// Status do gateway, agnóstico de provedor. Cada implementação mapeia
/// seus próprios códigos (PENDING, CONFIRMED, RECEIVED, OVERDUE...) para
/// um destes.
/// </summary>
public enum GatewayChargeStatus
{
    Unknown = 0,
    Pending = 1,
    Confirmed = 2,
    Received = 3,
    Overdue = 4,
    Refunded = 5,
    Failed = 6
}
