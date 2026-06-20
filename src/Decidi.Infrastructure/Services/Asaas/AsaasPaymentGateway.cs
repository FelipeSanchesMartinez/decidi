using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Decidi.Domain.Payments;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Decidi.Infrastructure.Services.Asaas;

/// <summary>
/// Implementação do IPaymentGateway sobre a API do Asaas. A configuração
/// (sandbox vs prod, chave) vem de AsaasOptions, ligado em DI a partir do
/// appsettings na seção "Asaas".
///
/// O HttpClient é injetado já configurado pelo IHttpClientFactory — não
/// criamos um por chamada para evitar socket exhaustion.
/// </summary>
public sealed class AsaasPaymentGateway : IPaymentGateway
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _http;
    private readonly AsaasOptions _opts;
    private readonly ILogger<AsaasPaymentGateway> _log;

    public string ProviderName => "asaas";

    public bool IsAvailable => _opts.IsConfigured;

    public AsaasPaymentGateway(
        HttpClient http,
        IOptions<AsaasOptions> opts,
        ILogger<AsaasPaymentGateway> log)
    {
        _http = http;
        _opts = opts.Value;
        _log = log;
    }

    public async Task<ChargeCreationResult> CreatePixChargeAsync(
        ChargeRequest request,
        CancellationToken ct = default)
    {
        EnsureConfigured();

        // 1. Garante o customer no Asaas (idempotente via externalReference = email).
        var customer = await UpsertCustomerAsync(request, ct);

        // 2. Cria a cobrança PIX com vencimento na data informada.
        var paymentReq = new AsaasPaymentRequest
        {
            Customer = customer,
            BillingType = "PIX",
            Value = request.Amount,
            DueDate = request.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            Description = Truncate(request.Description, 500),
            ExternalReference = request.PaymentId.ToString(),
        };

        using var payRes = await _http.PostAsJsonAsync("payments", paymentReq, JsonOpts, ct);
        var payment = await ReadOrThrowAsync<AsaasPaymentResponse>(payRes, "criar pagamento", ct);

        // 3. Busca o QR Code PIX gerado para esta cobrança.
        using var qrRes = await _http.GetAsync($"payments/{payment.Id}/pixQrCode", ct);
        var qr = await ReadOrThrowAsync<AsaasPixQrCodeResponse>(qrRes, "obter qrcode pix", ct);

        return new ChargeCreationResult(
            ChargeId: payment.Id,
            PixQrCodeBase64: qr.EncodedImage,
            PixCopyPaste: qr.Payload,
            PixExpiresAt: qr.ExpirationDate,
            Status: MapStatus(payment.Status),
            CheckoutUrl: payment.InvoiceUrl);
    }

    public async Task<ChargeSnapshot> GetChargeAsync(
        string chargeId,
        CancellationToken ct = default)
    {
        EnsureConfigured();
        using var res = await _http.GetAsync($"payments/{chargeId}", ct);
        var payment = await ReadOrThrowAsync<AsaasPaymentResponse>(res, "consultar pagamento", ct);
        return new ChargeSnapshot(
            ChargeId: payment.Id,
            Status: MapStatus(payment.Status),
            Amount: payment.Value,
            NetAmount: payment.NetValue,
            PaidAt: payment.PaymentDate);
    }

    public async Task<RefundResult> RefundChargeAsync(
        string chargeId,
        CancellationToken ct = default)
    {
        EnsureConfigured();
        // Asaas estorna pelo POST /payments/{id}/refund (body opcional para
        // motivo). Estornos PIX confirmados não precisam de valor — sempre
        // total. Boletos/cartão usam outro fluxo (não cobrimos aqui ainda).
        using var res = await _http.PostAsync($"payments/{chargeId}/refund", content: null, ct);

        if (!res.IsSuccessStatusCode)
        {
            var error = await ReadErrorAsync(res, ct);
            return new RefundResult(chargeId, false, null, error);
        }

        var refund = await res.Content.ReadFromJsonAsync<AsaasRefundResponse>(JsonOpts, ct);
        var entry = refund?.Refunds?.FirstOrDefault();
        return new RefundResult(
            ChargeId: chargeId,
            Succeeded: true,
            RefundedAt: entry?.DateCreated ?? DateTime.UtcNow,
            FailureReason: null);
    }

    public bool ValidateWebhookToken(string? receivedToken)
    {
        if (string.IsNullOrEmpty(_opts.WebhookToken))
        {
            // Sem token configurado = rejeita tudo. Operador precisa setá-lo
            // explicitamente — evita aceitar webhooks anônimos por acidente.
            _log.LogWarning("Webhook recebido mas Asaas:WebhookToken não está configurado.");
            return false;
        }
        return CryptographicEquals(_opts.WebhookToken, receivedToken ?? "");
    }

    public GatewayEvent? ParseWebhookPayload(string payload)
    {
        try
        {
            var hook = JsonSerializer.Deserialize<AsaasWebhookPayload>(payload, JsonOpts);
            if (hook?.Payment is null || string.IsNullOrEmpty(hook.Event))
                return null;

            return new GatewayEvent(
                EventId: $"{hook.Event}:{hook.Payment.Id}:{(hook.DateCreated ?? DateTime.UtcNow):O}",
                ChargeId: hook.Payment.Id,
                NewStatus: MapStatus(hook.Payment.Status),
                Value: hook.Payment.Value,
                OccurredAt: hook.DateCreated ?? DateTime.UtcNow,
                RawPayload: payload);
        }
        catch (JsonException ex)
        {
            _log.LogWarning(ex, "Webhook Asaas com payload inválido.");
            return null;
        }
    }

    // ── helpers ────────────────────────────────────────────────────

    private async Task<string> UpsertCustomerAsync(ChargeRequest req, CancellationToken ct)
    {
        // Busca por externalReference para reutilizar o mesmo customer entre
        // cobranças do mesmo email. Asaas indexa por externalReference.
        using var search = await _http.GetAsync(
            $"customers?externalReference={Uri.EscapeDataString(req.CustomerEmail)}", ct);
        if (search.IsSuccessStatusCode)
        {
            using var doc = await JsonDocument.ParseAsync(
                await search.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            if (doc.RootElement.TryGetProperty("data", out var data)
                && data.ValueKind == JsonValueKind.Array
                && data.GetArrayLength() > 0
                && data[0].TryGetProperty("id", out var id))
            {
                return id.GetString() ?? throw new InvalidOperationException("customer.id vazio");
            }
        }

        var create = new AsaasCustomerRequest
        {
            Name = req.CustomerName,
            Email = req.CustomerEmail,
            CpfCnpj = req.CustomerCpfCnpj,
            ExternalReference = req.CustomerEmail,
        };
        using var res = await _http.PostAsJsonAsync("customers", create, JsonOpts, ct);
        var customer = await ReadOrThrowAsync<AsaasCustomerResponse>(res, "criar customer", ct);
        return customer.Id;
    }

    private async Task<T> ReadOrThrowAsync<T>(HttpResponseMessage res, string op, CancellationToken ct)
    {
        if (!res.IsSuccessStatusCode)
        {
            var error = await ReadErrorAsync(res, ct);
            _log.LogError("Falha ao {Op} no Asaas: {Status} — {Error}", op, (int)res.StatusCode, error);
            throw new PaymentGatewayException($"Asaas {op} falhou: {error}");
        }
        var parsed = await res.Content.ReadFromJsonAsync<T>(JsonOpts, ct);
        return parsed ?? throw new PaymentGatewayException($"Asaas {op}: resposta vazia.");
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage res, CancellationToken ct)
    {
        try
        {
            var err = await res.Content.ReadFromJsonAsync<AsaasErrorResponse>(JsonOpts, ct);
            var first = err?.Errors?.FirstOrDefault();
            return first is null ? $"HTTP {(int)res.StatusCode}" : $"{first.Code}: {first.Description}";
        }
        catch
        {
            return $"HTTP {(int)res.StatusCode}";
        }
    }

    private void EnsureConfigured()
    {
        if (!_opts.IsConfigured)
            throw new PaymentGatewayException(
                "Asaas não está configurado. Defina Asaas:ApiKey em appsettings/user-secrets.");
    }

    private static GatewayChargeStatus MapStatus(string asaasStatus) => asaasStatus switch
    {
        "PENDING" => GatewayChargeStatus.Pending,
        "AWAITING_RISK_ANALYSIS" => GatewayChargeStatus.Pending,
        "CONFIRMED" => GatewayChargeStatus.Confirmed,
        "RECEIVED" => GatewayChargeStatus.Received,
        "RECEIVED_IN_CASH" => GatewayChargeStatus.Received,
        "OVERDUE" => GatewayChargeStatus.Overdue,
        "REFUNDED" => GatewayChargeStatus.Refunded,
        "REFUND_REQUESTED" => GatewayChargeStatus.Refunded,
        "CHARGEBACK_REQUESTED" => GatewayChargeStatus.Failed,
        "CHARGEBACK_DISPUTE" => GatewayChargeStatus.Failed,
        "DUNNING_REQUESTED" => GatewayChargeStatus.Overdue,
        _ => GatewayChargeStatus.Unknown,
    };

    /// <summary>Comparação em tempo constante para tokens compartilhados.</summary>
    private static bool CryptographicEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++) diff |= a[i] ^ b[i];
        return diff == 0;
    }

    private static string Truncate(string s, int max)
        => s.Length <= max ? s : s[..max];

    /// <summary>
    /// Configuração padrão do HttpClient consumido pelo IHttpClientFactory.
    /// Chamado pela registration no DependencyInjection.cs.
    /// Assinatura segue a sobrecarga AddHttpClient(Action&lt;IServiceProvider, HttpClient&gt;).
    /// </summary>
    public static void ConfigureHttpClient(IServiceProvider sp, HttpClient client)
    {
        var opts = sp.GetRequiredService<IOptions<AsaasOptions>>().Value;
        var baseUrl = string.IsNullOrEmpty(opts.BaseUrl) ? "https://sandbox.asaas.com/api/v3" : opts.BaseUrl;
        if (!baseUrl.EndsWith('/')) baseUrl += "/";

        client.BaseAddress = new Uri(baseUrl);
        client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(opts.UserAgent);
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        if (!string.IsNullOrEmpty(opts.ApiKey))
            client.DefaultRequestHeaders.TryAddWithoutValidation("access_token", opts.ApiKey);
    }
}

// PaymentGatewayException vive em Decidi.Domain.Payments.
