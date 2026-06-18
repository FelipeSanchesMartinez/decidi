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
        var built = EmailTemplates.EmailConfirmation(userName, confirmationLink);
        await SendAsync(toEmail, "Confirme seu e-mail · decidi", built.Html, built.Text);
    }

    public async Task SendPasswordResetAsync(string toEmail, string userName, string resetLink)
    {
        var built = EmailTemplates.PasswordReset(userName, resetLink);
        await SendAsync(toEmail, "Redefinição de senha · decidi", built.Html, built.Text);
    }

    public async Task SendMarketplaceEventAsync(
        string toEmail, string userName, string subject, string preview, string body, string ctaLabel, string ctaUrl)
    {
        var built = EmailTemplates.MarketplaceEvent(userName, preview, body, ctaLabel, ctaUrl);
        await SendAsync(toEmail, $"{subject} · decidi", built.Html, built.Text);
    }

    private async Task SendAsync(string to, string subject, string html, string text)
    {
        var payload = new
        {
            from = _options.From,
            to = new[] { to },
            subject,
            html,
            text, // Versão texto para clientes que não renderizam HTML e para melhorar entregabilidade.
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
