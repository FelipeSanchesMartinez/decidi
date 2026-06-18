namespace Decidi.Application.Interfaces;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string toEmail, string userName, string confirmationLink);
    Task SendPasswordResetAsync(string toEmail, string userName, string resetLink);

    /// <summary>E-mail genérico de evento do marketplace (call-to-action).</summary>
    Task SendMarketplaceEventAsync(string toEmail, string userName, string subject, string preview, string body, string ctaLabel, string ctaUrl);
}
