namespace Decidi.Infrastructure.Services.Asaas;

public sealed class AsaasOptions
{
    public const string SectionName = "Asaas";

    /// <summary>Chave de API do Asaas. Em desenvolvimento usar user-secrets / env var.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// URL base da API. Sandbox: https://sandbox.asaas.com/api/v3
    /// Produção: https://api.asaas.com/api/v3
    /// </summary>
    public string BaseUrl { get; set; } = "https://sandbox.asaas.com/api/v3";

    /// <summary>
    /// Token compartilhado configurado no painel do Asaas em "Notificações
    /// por webhook". O Asaas o envia no header asaas-access-token.
    /// </summary>
    public string WebhookToken { get; set; } = string.Empty;

    /// <summary>
    /// Identificação enviada no User-Agent das chamadas — facilita debug do
    /// lado do Asaas e telemetria.
    /// </summary>
    public string UserAgent { get; set; } = "decidi-platform/1.0";

    /// <summary>Em segundos. Default 15s.</summary>
    public int TimeoutSeconds { get; set; } = 15;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(ApiKey) && !string.IsNullOrWhiteSpace(BaseUrl);
}
