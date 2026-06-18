namespace Decidi.Domain.Enums;

public enum ReviewVisibility
{
    /// <summary>Aguardando o par-companheiro avaliar (blind review).</summary>
    Pending = 0,
    /// <summary>Liberada — visível publicamente.</summary>
    Released = 1
}
