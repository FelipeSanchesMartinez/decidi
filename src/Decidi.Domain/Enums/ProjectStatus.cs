namespace Decidi.Domain.Enums;

public enum ProjectStatus
{
    // Aberto para propostas — estado inicial após publicar.
    // Mantém o valor 0 (antigo "Open") para compatibilidade com dados já persistidos.
    ReceivingProposals = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    Draft = 4,
    InNegotiation = 5,
    Contracted = 6
}
