namespace Decidi.Application.Interfaces;

public interface IContactGuard
{
    /// <summary>
    /// Substitui telefones, e-mails, links externos e menções a apps de mensageria por
    /// "[contato removido]". Retorna o texto sanitizado e uma flag indicando se houve remoção.
    /// </summary>
    (string Redacted, bool WasRedacted) Redact(string input);
}
