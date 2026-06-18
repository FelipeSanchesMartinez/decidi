using System.Text.RegularExpressions;
using Decidi.Application.Interfaces;

namespace Decidi.Application.Services;

public partial class ContactGuard : IContactGuard
{
    private const string Placeholder = "[contato removido]";

    // Telefones BR: +55 opcional, DDD com ou sem parênteses, 9 opcional, 8-9 dígitos
    // Exemplos: (11) 91234-5678, 11912345678, +5511912345678, 11 1234-5678
    [GeneratedRegex(@"(?:\+?55[\s.-]?)?\(?[1-9][0-9]\)?[\s.-]?9?[0-9]{4}[\s.-]?[0-9]{4}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex PhoneRegex();

    [GeneratedRegex(@"[\w.+-]+@[\w-]+\.[\w.-]+",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    // URLs de qualquer domínio com http(s) OU short urls (wa.me, t.me) sem scheme.
    [GeneratedRegex(@"(?:https?://|www\.)\S+|(?:\b(?:wa\.me|t\.me|bit\.ly|tinyurl\.com)/\S+)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex UrlRegex();

    // Palavras-chave de canais externos (case-insensitive, word-boundary).
    [GeneratedRegex(@"\b(whats?app|whats|zap|telegram|insta(gram)?|messenger|skype|discord|gmail|hotmail|outlook|yahoo)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex KeywordRegex();

    public (string Redacted, bool WasRedacted) Redact(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (input ?? string.Empty, false);

        var result = input;
        var redacted = false;

        var phoneNew = PhoneRegex().Replace(result, Placeholder);
        if (phoneNew != result) { result = phoneNew; redacted = true; }

        var emailNew = EmailRegex().Replace(result, Placeholder);
        if (emailNew != result) { result = emailNew; redacted = true; }

        var urlNew = UrlRegex().Replace(result, Placeholder);
        if (urlNew != result) { result = urlNew; redacted = true; }

        var kwNew = KeywordRegex().Replace(result, Placeholder);
        if (kwNew != result) { result = kwNew; redacted = true; }

        return (result, redacted);
    }
}
