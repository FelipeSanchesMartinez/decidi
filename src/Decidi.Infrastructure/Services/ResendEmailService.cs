using System.Net.Http.Headers;
using System.Net.Http.Json;
using Decidi.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Decidi.Infrastructure.Services;

public sealed class ResendEmailService : IEmailService
{
    private const int MaxRetries = 3;

    private readonly HttpClient _http;
    private readonly ResendOptions _options;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(HttpClient http, IOptions<ResendOptions> options, ILogger<ResendEmailService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink)
    {
        var subject = "Confirme seu e-mail - decidi";
        var html = $"""
            <div style="background-color:#f5f5f5;padding:32px 0;width:100%">
                <div style="font-family:'Nunito Sans',Inter,Arial,sans-serif;max-width:600px;margin:0 auto;padding:40px 32px;background-color:#ffffff;border-radius:12px">
                    <div style="text-align:center;margin-bottom:32px">
                        <h1 style="color:#0B6B4F;font-size:28px;margin:0">decidi</h1>
                    </div>
                    <h2 style="color:#1a1a1a;font-size:22px">Olá, {userName}!</h2>
                    <p style="color:#333333;font-size:16px;line-height:1.6">
                        Bem-vindo à decidi! Para ativar sua conta, confirme seu e-mail clicando no botão abaixo:
                    </p>
                    <div style="text-align:center;margin:32px 0">
                        <a href="{confirmationLink}"
                           style="background:linear-gradient(135deg,#0B6B4F,#1E56F2);color:#ffffff;text-decoration:none;padding:14px 32px;border-radius:8px;font-size:16px;font-weight:700;display:inline-block">
                            Confirmar E-mail
                        </a>
                    </div>
                    <p style="color:#666666;font-size:14px;line-height:1.5">
                        Se você não criou uma conta na decidi, ignore este e-mail.
                    </p>
                    <hr style="border:none;border-top:1px solid #eeeeee;margin:32px 0" />
                    <p style="color:#999999;font-size:12px;text-align:center">
                        decidi — A plataforma para quem decide fazer acontecer.
                    </p>
                </div>
            </div>
            """;

        await SendAsync(toEmail, subject, html);
    }

    public async Task SendPasswordResetAsync(string toEmail, string userName, string resetLink)
    {
        var subject = "Recuperar senha - decidi";
        var html = $"""
            <div style="background-color:#f5f5f5;padding:32px 0;width:100%">
                <div style="font-family:'Nunito Sans',Inter,Arial,sans-serif;max-width:600px;margin:0 auto;padding:40px 32px;background-color:#ffffff;border-radius:12px">
                    <div style="text-align:center;margin-bottom:32px">
                        <h1 style="color:#0B6B4F;font-size:28px;margin:0">decidi</h1>
                    </div>
                    <h2 style="color:#1a1a1a;font-size:22px">Olá, {userName}!</h2>
                    <p style="color:#333333;font-size:16px;line-height:1.6">
                        Recebemos uma solicitação para redefinir sua senha. Clique no botão abaixo para criar uma nova:
                    </p>
                    <div style="text-align:center;margin:32px 0">
                        <a href="{resetLink}"
                           style="background:linear-gradient(135deg,#0B6B4F,#1E56F2);color:#ffffff;text-decoration:none;padding:14px 32px;border-radius:8px;font-size:16px;font-weight:700;display:inline-block">
                            Redefinir Senha
                        </a>
                    </div>
                    <p style="color:#666666;font-size:14px;line-height:1.5">
                        Este link expira em 1 hora. Se você não solicitou a redefinição, ignore este e-mail.
                    </p>
                    <hr style="border:none;border-top:1px solid #eeeeee;margin:32px 0" />
                    <p style="color:#999999;font-size:12px;text-align:center">
                        decidi — A plataforma para quem decide fazer acontecer.
                    </p>
                </div>
            </div>
            """;

        await SendAsync(toEmail, subject, html);
    }

    public async Task SendMarketplaceEventAsync(
        string toEmail, string userName, string subject, string preview, string body, string ctaLabel, string ctaUrl)
    {
        var html = $"""
            <div style="background-color:#f5f5f5;padding:32px 0;width:100%">
                <div style="font-family:'Nunito Sans',Inter,Arial,sans-serif;max-width:600px;margin:0 auto;padding:40px 32px;background-color:#ffffff;border-radius:12px">
                    <div style="text-align:center;margin-bottom:32px">
                        <h1 style="color:#0B6B4F;font-size:28px;margin:0">decidi</h1>
                    </div>
                    <h2 style="color:#1a1a1a;font-size:22px">Olá, {userName}!</h2>
                    <p style="color:#555;font-size:15px;line-height:1.5;margin:0 0 18px 0">{preview}</p>
                    <p style="color:#333333;font-size:16px;line-height:1.6">{body}</p>
                    <div style="text-align:center;margin:32px 0">
                        <a href="{ctaUrl}"
                           style="background:linear-gradient(135deg,#0B6B4F,#1E56F2);color:#ffffff;text-decoration:none;padding:14px 32px;border-radius:8px;font-size:16px;font-weight:700;display:inline-block">
                            {ctaLabel}
                        </a>
                    </div>
                    <p style="color:#666666;font-size:13px;line-height:1.5">
                        Você está recebendo este e-mail porque tem uma conta na decidi.
                    </p>
                    <hr style="border:none;border-top:1px solid #eeeeee;margin:32px 0" />
                    <p style="color:#999999;font-size:12px;text-align:center">
                        decidi — A plataforma para quem decide fazer acontecer.
                    </p>
                </div>
            </div>
            """;

        await SendAsync(toEmail, subject, html);
    }

    private async Task SendAsync(string to, string subject, string html)
    {
        var payload = new
        {
            from = _options.From,
            to = new[] { to },
            subject,
            html,
        };

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "emails")
                {
                    Content = JsonContent.Create(payload),
                };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

                using var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                    return;

                var body = await response.Content.ReadAsStringAsync();

                if ((int)response.StatusCode < 500)
                {
                    _logger.LogWarning(
                        "[Resend] envio para {To} falhou com {Status}: {Body}",
                        to, response.StatusCode, body);
                    return;
                }

                _logger.LogWarning(
                    "[Resend] tentativa {Attempt}/{Max} para {To} retornou {Status}: {Body}",
                    attempt, MaxRetries, to, response.StatusCode, body);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning(ex,
                    "[Resend] tentativa {Attempt}/{Max} para {To} lançou exceção.",
                    attempt, MaxRetries, to);
            }
        }

        _logger.LogError("[Resend] desistiu de enviar para {To} após {Max} tentativas.", to, MaxRetries);
    }
}
