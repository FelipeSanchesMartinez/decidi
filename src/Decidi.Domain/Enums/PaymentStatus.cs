namespace Decidi.Domain.Enums;

public enum PaymentStatus
{
    /// <summary>Aguardando charge do cliente no gateway.</summary>
    Pending = 0,
    /// <summary>Cliente pagou; valor retido em escrow pela plataforma.</summary>
    Escrowed = 1,
    /// <summary>Valor liberado ao profissional.</summary>
    Released = 2,
    /// <summary>Disputa aberta — valor congelado até resolução.</summary>
    Disputed = 3,
    /// <summary>Estornado integralmente ao cliente.</summary>
    Refunded = 4,
    /// <summary>Cancelado antes de qualquer cobrança.</summary>
    Cancelled = 5
}
