using System.Net;
using System.Text;

namespace Decidi.Infrastructure.Services;

/// <summary>
/// Templates HTML + texto plano para e-mails transacionais.
/// Inline styles obrigatórios (Gmail descarta &lt;style&gt;).
/// Identidade visual: gradient verde→azul no logo e no CTA, Nunito Sans/Inter com fallback robusto.
/// </summary>
internal static class EmailTemplates
{
    // Tokens da marca espelhados manualmente (CSS variables não funcionam em e-mail).
    private const string BrandGreen = "#0B6B4F";
    private const string BrandTeal = "#18A7A3";
    private const string BrandBlue = "#1E56F2";
    private const string Gradient = "linear-gradient(90deg,#0B6B4F 0%,#18A7A3 45%,#1E56F2 100%)";
    private const string TextDark = "#0F172A";
    private const string TextBody = "#334155";
    private const string TextMuted = "#64748B";
    private const string TextFooter = "#94A3B8";
    private const string Divider = "#E2E8F0";
    private const string PageBg = "#F8FAFC";
    private const string CardBg = "#FFFFFF";

    private const string FontStack =
        "'Nunito Sans',-apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,'Helvetica Neue',Arial,sans-serif";

    public sealed record Built(string Html, string Text);

    public static Built EmailConfirmation(string userName, string confirmationLink)
        => Build(
            preheader: "Confirme seu e-mail para começar a usar a decidi.",
            greeting: $"Olá, {userName}!",
            lead: "Quase lá. Confirme seu e-mail para liberar todas as funcionalidades da decidi — publicar projetos, enviar propostas e contratar com segurança.",
            ctaLabel: "Confirmar e-mail",
            ctaUrl: confirmationLink,
            extra: "O link vale por 24 horas. Se você não criou uma conta na decidi, pode ignorar esta mensagem com segurança.");

    public static Built PasswordReset(string userName, string resetLink)
        => Build(
            preheader: "Crie uma nova senha para a sua conta na decidi.",
            greeting: $"Olá, {userName}!",
            lead: "Recebemos uma solicitação para redefinir a senha da sua conta. Clique no botão abaixo para criar uma nova.",
            ctaLabel: "Redefinir senha",
            ctaUrl: resetLink,
            extra: "Por segurança, este link expira em 1 hora. Se não foi você que pediu, pode ignorar este e-mail — sua senha atual continua valendo.");

    public static Built MarketplaceEvent(
        string userName, string preview, string body, string ctaLabel, string ctaUrl)
        => Build(
            preheader: preview,
            greeting: $"Olá, {userName}!",
            lead: preview,
            additional: body,
            ctaLabel: ctaLabel,
            ctaUrl: ctaUrl);

    private static Built Build(
        string preheader,
        string greeting,
        string lead,
        string ctaLabel,
        string ctaUrl,
        string? additional = null,
        string? extra = null)
    {
        var html = BuildHtml(preheader, greeting, lead, additional, ctaLabel, ctaUrl, extra);
        var text = BuildText(greeting, lead, additional, ctaLabel, ctaUrl, extra);
        return new Built(html, text);
    }

    private static string BuildHtml(
        string preheader, string greeting, string lead, string? additional,
        string ctaLabel, string ctaUrl, string? extra)
    {
        var sb = new StringBuilder(2048);
        var safeLink = WebUtility.HtmlEncode(ctaUrl);

        sb.Append("<!DOCTYPE html><html lang=\"pt-BR\"><head><meta charset=\"utf-8\">");
        sb.Append("<meta name=\"viewport\" content=\"width=device-width,initial-scale=1\">");
        sb.Append($"<title>{WebUtility.HtmlEncode(greeting)}</title></head>");
        sb.Append($"<body style=\"margin:0;padding:0;background:{PageBg};font-family:{FontStack};color:{TextBody}\">");

        // Preheader oculto: texto que aparece na lista do inbox, mas não no corpo.
        sb.Append("<div style=\"display:none;max-height:0;overflow:hidden;mso-hide:all;font-size:1px;line-height:1px;color:");
        sb.Append(PageBg).Append("\">");
        sb.Append(WebUtility.HtmlEncode(preheader));
        // Espaços não-quebráveis empurram o preview text para fora da janela do preheader.
        sb.Append(new string(' ', 80));
        sb.Append("</div>");

        // Container externo (full-bleed background).
        sb.Append($"<table role=\"presentation\" width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"background:{PageBg}\"><tr><td align=\"center\" style=\"padding:32px 16px\">");

        // Card central.
        sb.Append($"<table role=\"presentation\" width=\"600\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"max-width:600px;width:100%;background:{CardBg};border-radius:20px;border:1px solid {Divider};overflow:hidden\">");

        // Header com logo em gradient.
        sb.Append("<tr><td style=\"padding:32px 32px 8px;text-align:center\">");
        sb.Append($"<span style=\"display:inline-block;font-family:{FontStack};font-size:32px;font-weight:800;letter-spacing:-0.5px;background:{Gradient};-webkit-background-clip:text;background-clip:text;color:{BrandGreen};-webkit-text-fill-color:transparent\">decidi</span>");
        sb.Append("</td></tr>");

        // Corpo.
        sb.Append("<tr><td style=\"padding:8px 40px 0\">");
        sb.Append($"<h1 style=\"margin:0 0 16px;font-family:{FontStack};font-size:22px;font-weight:700;color:{TextDark};line-height:1.3\">");
        sb.Append(WebUtility.HtmlEncode(greeting));
        sb.Append("</h1>");
        sb.Append($"<p style=\"margin:0 0 16px;font-size:15px;line-height:1.6;color:{TextBody}\">");
        sb.Append(WebUtility.HtmlEncode(lead));
        sb.Append("</p>");

        if (!string.IsNullOrEmpty(additional))
        {
            sb.Append($"<p style=\"margin:0 0 16px;font-size:15px;line-height:1.6;color:{TextBody}\">");
            sb.Append(WebUtility.HtmlEncode(additional));
            sb.Append("</p>");
        }
        sb.Append("</td></tr>");

        // CTA — usa "bulletproof button" (tabela + VML p/ Outlook) num approach mínimo: tabela com inline gradient.
        sb.Append("<tr><td style=\"padding:8px 40px 24px\" align=\"center\">");
        sb.Append("<table role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td align=\"center\" ");
        sb.Append($"style=\"background:{BrandBlue};background:{Gradient};border-radius:12px\">");
        sb.Append($"<a href=\"{safeLink}\" target=\"_blank\" rel=\"noopener\" ");
        sb.Append($"style=\"display:inline-block;padding:14px 32px;font-family:{FontStack};font-size:15px;font-weight:700;color:#FFFFFF;text-decoration:none;border-radius:12px;mso-padding-alt:0\">");
        sb.Append(WebUtility.HtmlEncode(ctaLabel));
        sb.Append("</a>");
        sb.Append("</td></tr></table>");
        sb.Append("</td></tr>");

        // Fallback link (caso o botão não funcione em algum cliente).
        sb.Append("<tr><td style=\"padding:0 40px 24px\">");
        sb.Append($"<p style=\"margin:0;font-size:13px;line-height:1.5;color:{TextMuted}\">");
        sb.Append("Se o botão não funcionar, copie e cole este endereço no navegador:<br>");
        sb.Append($"<a href=\"{safeLink}\" target=\"_blank\" rel=\"noopener\" style=\"color:{BrandBlue};text-decoration:underline;word-break:break-all\">");
        sb.Append(WebUtility.HtmlEncode(ctaUrl));
        sb.Append("</a>");
        sb.Append("</p>");
        sb.Append("</td></tr>");

        if (!string.IsNullOrEmpty(extra))
        {
            sb.Append("<tr><td style=\"padding:0 40px 24px\">");
            sb.Append($"<p style=\"margin:0;font-size:13px;line-height:1.5;color:{TextMuted}\">");
            sb.Append(WebUtility.HtmlEncode(extra));
            sb.Append("</p></td></tr>");
        }

        // Divisor.
        sb.Append($"<tr><td style=\"padding:0 40px\"><hr style=\"border:none;border-top:1px solid {Divider};margin:0\"></td></tr>");

        // Footer institucional.
        sb.Append("<tr><td style=\"padding:24px 40px 32px;text-align:center\">");
        sb.Append($"<p style=\"margin:0 0 8px;font-family:{FontStack};font-size:13px;font-weight:600;color:{TextDark}\">decidi</p>");
        sb.Append($"<p style=\"margin:0 0 12px;font-size:12px;line-height:1.5;color:{TextMuted}\">O jeito simples de contratar freelancers.</p>");
        sb.Append($"<p style=\"margin:0 0 12px;font-size:12px;line-height:1.5;color:{TextFooter}\">");
        sb.Append("Você está recebendo este e-mail porque tem uma conta na decidi.<br>");
        sb.Append("Contato: <a href=\"mailto:contato@decidi.com.br\" style=\"color:");
        sb.Append(TextMuted).Append(";text-decoration:underline\">contato@decidi.com.br</a> · ");
        sb.Append("<a href=\"https://decidi.com.br/privacidade\" style=\"color:");
        sb.Append(TextMuted).Append(";text-decoration:underline\">Política de privacidade</a>");
        sb.Append("</p>");
        sb.Append("</td></tr>");

        sb.Append("</table>"); // card
        sb.Append("</td></tr></table>"); // outer
        sb.Append("</body></html>");

        return sb.ToString();
    }

    private static string BuildText(
        string greeting, string lead, string? additional,
        string ctaLabel, string ctaUrl, string? extra)
    {
        var sb = new StringBuilder(512);
        sb.AppendLine(greeting);
        sb.AppendLine();
        sb.AppendLine(lead);
        sb.AppendLine();
        if (!string.IsNullOrEmpty(additional))
        {
            sb.AppendLine(additional);
            sb.AppendLine();
        }
        sb.AppendLine($"{ctaLabel}:");
        sb.AppendLine(ctaUrl);
        sb.AppendLine();
        if (!string.IsNullOrEmpty(extra))
        {
            sb.AppendLine(extra);
            sb.AppendLine();
        }
        sb.AppendLine("---");
        sb.AppendLine("decidi — O jeito simples de contratar freelancers.");
        sb.AppendLine("Contato: contato@decidi.com.br");
        sb.AppendLine("Política de privacidade: https://decidi.com.br/privacidade");
        return sb.ToString();
    }
}
