using System.Text;
using Decidi.Application.Interfaces;
using Decidi.Domain.Payments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/webhooks")]
[AllowAnonymous]
public class WebhooksController(
    IPaymentGateway gateway,
    IPaymentEventHandler eventHandler,
    ILogger<WebhooksController> logger) : ControllerBase
{
    /// <summary>
    /// Webhook do Asaas. Configure a URL no painel do Asaas em
    /// "Notificações por webhook" e o mesmo token em Asaas:WebhookToken
    /// do appsettings.
    ///
    /// Princípios:
    ///  - Sempre responde 200 quando o token é válido e o payload faz sentido,
    ///    mesmo que o evento seja desconhecido. Isso impede o Asaas de
    ///    reentregar o mesmo evento indefinidamente.
    ///  - 401 só quando o token não bate (suspeita de ataque).
    /// </summary>
    [HttpPost("asaas")]
    public async Task<IActionResult> Asaas(CancellationToken ct)
    {
        // O token chega no header asaas-access-token, segundo a doc do Asaas.
        var token = Request.Headers["asaas-access-token"].FirstOrDefault();
        if (!gateway.ValidateWebhookToken(token))
        {
            logger.LogWarning("Webhook do Asaas com token inválido — rejeitado.");
            return Unauthorized();
        }

        Request.EnableBuffering();
        string body;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            body = await reader.ReadToEndAsync(ct);
            Request.Body.Position = 0;
        }

        var evt = gateway.ParseWebhookPayload(body);
        if (evt is null)
        {
            // Payload válido por token mas que o gateway não consegue normalizar
            // (ex: PAYMENT_CREATED sem dados do payment). Ack para não retentar.
            return Ok(new { received = true, processed = false });
        }

        var outcome = await eventHandler.HandleAsync(evt, ct);
        return Ok(new
        {
            received = true,
            outcome = outcome.ToString(),
            chargeId = evt.ChargeId,
        });
    }
}
