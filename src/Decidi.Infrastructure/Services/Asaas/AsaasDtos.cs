using System.Text.Json.Serialization;

namespace Decidi.Infrastructure.Services.Asaas;

// DTOs internos da API Asaas. Mantemos isolados aqui para que o resto do
// código nunca veja o formato do gateway — apenas os tipos do Domain.

internal sealed class AsaasCustomerRequest
{
    [JsonPropertyName("name")] public string Name { get; set; } = "";
    [JsonPropertyName("email")] public string Email { get; set; } = "";
    [JsonPropertyName("cpfCnpj")] public string? CpfCnpj { get; set; }
    [JsonPropertyName("externalReference")] public string? ExternalReference { get; set; }
}

internal sealed class AsaasCustomerResponse
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
}

internal sealed class AsaasPaymentRequest
{
    [JsonPropertyName("customer")] public string Customer { get; set; } = "";
    [JsonPropertyName("billingType")] public string BillingType { get; set; } = "PIX";
    [JsonPropertyName("value")] public decimal Value { get; set; }
    [JsonPropertyName("dueDate")] public string DueDate { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
    [JsonPropertyName("externalReference")] public string ExternalReference { get; set; } = "";
}

internal sealed class AsaasPaymentResponse
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("value")] public decimal Value { get; set; }
    [JsonPropertyName("netValue")] public decimal NetValue { get; set; }
    [JsonPropertyName("invoiceUrl")] public string? InvoiceUrl { get; set; }
    [JsonPropertyName("dateCreated")] public DateTime? DateCreated { get; set; }
    [JsonPropertyName("paymentDate")] public DateTime? PaymentDate { get; set; }
}

internal sealed class AsaasPixQrCodeResponse
{
    [JsonPropertyName("encodedImage")] public string EncodedImage { get; set; } = "";
    [JsonPropertyName("payload")] public string Payload { get; set; } = "";
    [JsonPropertyName("expirationDate")] public DateTime ExpirationDate { get; set; }
}

internal sealed class AsaasRefundResponse
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("refunds")] public List<AsaasRefundEntry>? Refunds { get; set; }
}

internal sealed class AsaasRefundEntry
{
    [JsonPropertyName("status")] public string Status { get; set; } = "";
    [JsonPropertyName("dateCreated")] public DateTime? DateCreated { get; set; }
    [JsonPropertyName("description")] public string? Description { get; set; }
}

internal sealed class AsaasWebhookPayload
{
    [JsonPropertyName("event")] public string Event { get; set; } = "";
    [JsonPropertyName("payment")] public AsaasPaymentResponse? Payment { get; set; }
    [JsonPropertyName("dateCreated")] public DateTime? DateCreated { get; set; }
}

internal sealed class AsaasErrorResponse
{
    [JsonPropertyName("errors")] public List<AsaasErrorEntry>? Errors { get; set; }
}

internal sealed class AsaasErrorEntry
{
    [JsonPropertyName("code")] public string Code { get; set; } = "";
    [JsonPropertyName("description")] public string Description { get; set; } = "";
}
