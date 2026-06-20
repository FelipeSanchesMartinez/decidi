namespace Decidi.Application.DTOs.Payments;

/// <summary>
/// Dados de uma cobrança PIX exibida para o cliente: QR Code, copia-cola,
/// valor, expiração e o GatewayRef pra reconciliar manualmente se preciso.
/// </summary>
public sealed record PixChargeDto(
    string GatewayRef,
    string QrCodeBase64,
    string CopyPaste,
    decimal Amount,
    DateTime ExpiresAt);
