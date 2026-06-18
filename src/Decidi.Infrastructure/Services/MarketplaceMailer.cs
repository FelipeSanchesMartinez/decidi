using Decidi.Application.DTOs.Notifications;
using Decidi.Application.Interfaces;
using Decidi.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Decidi.Infrastructure.Services;

public class MarketplaceMailer(
    UserManager<ApplicationUser> userManager,
    IEmailService emailService,
    INotificationPreferencesService preferencesService,
    ILogger<MarketplaceMailer> logger) : IMarketplaceMailer
{
    public async Task ProposalReceivedAsync(string clientUserId, string projectTitle, string freelancerName, Guid projectId, string webBaseUrl)
    {
        if (!await IsEmailAllowed(clientUserId, p => p.EmailProposalReceived)) return;
        var user = await ResolveAsync(clientUserId);
        if (user is null) return;

        await SafeSendAsync(user, "Você recebeu uma nova proposta",
            $"{freelancerName} enviou uma proposta para \"{projectTitle}\".",
            "Acesse seu projeto para revisar a proposta, comparar com outras e decidir com calma.",
            "Ver propostas",
            $"{webBaseUrl}/projects/{projectId}");
    }

    public async Task ProposalAcceptedAsync(string freelancerUserId, string projectTitle, Guid projectId, string webBaseUrl)
    {
        if (!await IsEmailAllowed(freelancerUserId, p => p.EmailProposalAccepted)) return;
        var user = await ResolveAsync(freelancerUserId);
        if (user is null) return;

        await SafeSendAsync(user, "Sua proposta foi aceita!",
            $"Parabéns! Sua proposta para \"{projectTitle}\" foi aceita.",
            "Acesse o projeto para iniciar a conversa com o cliente e combinar os próximos passos.",
            "Abrir projeto",
            $"{webBaseUrl}/projects/{projectId}");
    }

    public async Task ProposalRejectedAsync(string freelancerUserId, string projectTitle, string webBaseUrl)
    {
        if (!await IsEmailAllowed(freelancerUserId, p => p.EmailProposalRejected)) return;
        var user = await ResolveAsync(freelancerUserId);
        if (user is null) return;

        await SafeSendAsync(user, "Sua proposta não foi selecionada",
            $"Sua proposta para \"{projectTitle}\" não foi escolhida desta vez.",
            "Não desanime — continue enviando boas propostas. Você pode revisar suas propostas e buscar novos projetos.",
            "Buscar projetos",
            $"{webBaseUrl}/projects");
    }

    public async Task ProjectCompletedAsync(string toUserId, string projectTitle, Guid projectId, string webBaseUrl)
    {
        if (!await IsEmailAllowed(toUserId, p => p.EmailProjectCompleted)) return;
        var user = await ResolveAsync(toUserId);
        if (user is null) return;

        await SafeSendAsync(user, "Projeto concluído — deixe sua avaliação",
            $"O projeto \"{projectTitle}\" foi marcado como concluído.",
            "Sua avaliação ajuda outras pessoas da comunidade a tomarem decisões melhores. Leva menos de 1 minuto.",
            "Avaliar agora",
            $"{webBaseUrl}/projects/{projectId}");
    }

    private async Task<bool> IsEmailAllowed(string userId, Func<NotificationPreferencesDto, bool> selector)
    {
        try
        {
            var prefs = await preferencesService.GetForUserAsync(userId);
            return selector(prefs);
        }
        catch
        {
            return true; // se prefs indisponível, defaulta para enviar (não bloquear).
        }
    }

    private async Task<ApplicationUser?> ResolveAsync(string userId)
    {
        try
        {
            var user = await userManager.FindByIdAsync(userId);
            if (user is null || string.IsNullOrEmpty(user.Email) || !user.EmailConfirmed)
                return null;
            return user;
        }
        catch
        {
            return null;
        }
    }

    private async Task SafeSendAsync(ApplicationUser user, string subject, string preview, string body, string ctaLabel, string ctaUrl)
    {
        try
        {
            await emailService.SendMarketplaceEventAsync(user.Email!, user.FullName, subject, preview, body, ctaLabel, ctaUrl);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao enviar e-mail '{Subject}' para {UserId}", subject, user.Id);
        }
    }
}
