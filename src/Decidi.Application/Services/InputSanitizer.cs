using Decidi.Application.Interfaces;
using Ganss.Xss;

namespace Decidi.Application.Services;

public class InputSanitizer : ISanitizer
{
    private readonly HtmlSanitizer _sanitizer;

    public InputSanitizer()
    {
        _sanitizer = new HtmlSanitizer();
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedCssProperties.Clear();
        _sanitizer.AllowedSchemes.Clear();
    }

    public string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return _sanitizer.Sanitize(input);
    }
}
