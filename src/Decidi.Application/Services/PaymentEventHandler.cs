using Decidi.Application.Interfaces;
using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Decidi.Domain.Interfaces;
using Decidi.Domain.Payments;
using Microsoft.Extensions.Logging;

namespace Decidi.Application.Services;

/// <summary>
/// Aplica eventos do gateway no estado dos Payments do domínio. Pensado
/// para ser chamado pelo WebhooksController, mas pode ser reaproveitado
/// por um job de reconciliação que lê o gateway diretamente.
///
/// Idempotência: a comparação é feita pelo status atual do Payment. Se o
/// novo status já está aplicado (ex: webhook duplicado), retorna NoChange
/// sem disparar notificações nem persistir. Suficiente para PIX, onde o
/// fluxo de status é monotônico (Pending → Escrowed → Released/Refunded).
/// </summary>
public sealed class PaymentEventHandler(
    IRepository<Payment> paymentRepository,
    INotificationService notificationService,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork,
    ILogger<PaymentEventHandler> logger) : IPaymentEventHandler
{
    public async Task<PaymentEventOutcome> HandleAsync(GatewayEvent evt, CancellationToken ct = default)
    {
        var match = (await paymentRepository.FindAsync(p => p.GatewayRef == evt.ChargeId)).FirstOrDefault();
        if (match is null)
        {
            // Webhook pode chegar de cobrança que NÃO criamos (cliente do
            // Asaas pagou outro produto). Não é erro — apenas ignoramos.
            logger.LogInformation(
                "Webhook do gateway para chargeId {ChargeId} sem Payment correspondente — ignorado.",
                evt.ChargeId);
            return PaymentEventOutcome.Unknown;
        }

        switch (evt.NewStatus)
        {
            case GatewayChargeStatus.Confirmed:
            case GatewayChargeStatus.Received:
                return await ApplyConfirmedAsync(match, evt, ct);

            case GatewayChargeStatus.Refunded:
                return await ApplyRefundedAsync(match, evt, ct);

            case GatewayChargeStatus.Overdue:
                // Não muda o status do Payment — cliente pode pagar atrasado
                // e cair em CONFIRMED depois. Só logamos para visibilidade.
                logger.LogWarning(
                    "Cobrança {ChargeId} marcada como OVERDUE no gateway. Payment {PaymentId}.",
                    evt.ChargeId, match.Id);
                return PaymentEventOutcome.NoChange;

            case GatewayChargeStatus.Failed:
                logger.LogError(
                    "Cobrança {ChargeId} falhou no gateway. Payment {PaymentId} fica em {Status}.",
                    evt.ChargeId, match.Id, match.Status);
                return PaymentEventOutcome.NoChange;

            default:
                logger.LogDebug(
                    "Evento de gateway com status {Status} ignorado para charge {ChargeId}.",
                    evt.NewStatus, evt.ChargeId);
                return PaymentEventOutcome.NoChange;
        }
    }

    private async Task<PaymentEventOutcome> ApplyConfirmedAsync(
        Payment payment, GatewayEvent evt, CancellationToken ct)
    {
        if (payment.Status >= PaymentStatus.Escrowed)
        {
            // Já está em Escrowed/Released/etc — webhook duplicado. Ack sem fazer nada.
            return PaymentEventOutcome.NoChange;
        }

        payment.Status = PaymentStatus.Escrowed;
        payment.EscrowedAt = evt.OccurredAt;
        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync();

        // Busca título do projeto pra notificação humana — falhas aqui não
        // quebram a confirmação do pagamento, que já foi persistida.
        string projectTitle = "seu projeto";
        try
        {
            var project = await projectRepository.GetByIdAsync(payment.ProjectId);
            if (project is not null) projectTitle = project.Title;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao buscar título do projeto {ProjectId} para notificação.", payment.ProjectId);
        }

        await notificationService.CreateAsync(
            payment.ClientId, "payment_confirmed",
            "Pagamento confirmado",
            $"Recebemos seu pagamento para \"{projectTitle}\". A contratação está oficialmente ativa.",
            $"/projects/{payment.ProjectId}");

        await notificationService.CreateAsync(
            payment.FreelancerId, "contract_active",
            "Contrato ativo",
            $"O cliente confirmou o pagamento da taxa de contratação de \"{projectTitle}\". Você já pode começar.",
            $"/projects/{payment.ProjectId}");

        logger.LogInformation(
            "Payment {PaymentId} para charge {ChargeId} marcado como Escrowed.",
            payment.Id, evt.ChargeId);

        return PaymentEventOutcome.Applied;
    }

    private async Task<PaymentEventOutcome> ApplyRefundedAsync(
        Payment payment, GatewayEvent evt, CancellationToken ct)
    {
        if (payment.Status == PaymentStatus.Refunded) return PaymentEventOutcome.NoChange;

        payment.Status = PaymentStatus.Refunded;
        paymentRepository.Update(payment);
        await unitOfWork.SaveChangesAsync();

        await notificationService.CreateAsync(
            payment.ClientId, "payment_refunded",
            "Pagamento estornado",
            "O valor da contratação foi estornado integralmente.",
            $"/projects/{payment.ProjectId}");

        await notificationService.CreateAsync(
            payment.FreelancerId, "contract_refunded",
            "Contrato cancelado",
            "A cobrança da taxa de contratação foi estornada. O contrato foi encerrado.",
            $"/projects/{payment.ProjectId}");

        logger.LogInformation(
            "Payment {PaymentId} marcado como Refunded.", payment.Id);
        return PaymentEventOutcome.Applied;
    }
}
