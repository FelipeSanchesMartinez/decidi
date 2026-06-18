namespace Decidi.Domain.Enums;

public enum ProposalStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Withdrawn = 3,
    // Aceite de outra proposta no mesmo projeto encerra esta — não é uma rejeição manual.
    ClosedByContract = 4
}
