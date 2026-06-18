using Decidi.Application.DTOs.Proposals;
using Decidi.Application.Interfaces;
using Decidi.Application.Mappings;
using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Decidi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Decidi.Application.Services;

public class ProposalService(
    IProposalRepository proposalRepository,
    IProjectRepository projectRepository,
    IConversationRepository conversationRepository,
    IRepository<Payment> paymentRepository,
    IPlatformFeeService platformFeeService,
    INotificationService notificationService,
    IMarketplaceMailer mailer,
    ISanitizer sanitizer,
    IConfiguration configuration,
    IUnitOfWork unitOfWork) : IProposalService
{
    private string WebBaseUrl => configuration["WebBaseUrl"] ?? "https://decidi.com.br";

    public async Task<ProposalDto> CreateAsync(CreateProposalRequest request, string freelancerId)
    {
        request.CoverLetter = sanitizer.Sanitize(request.CoverLetter);
        var project = await projectRepository.GetByIdAsync(request.ProjectId)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        if (project.Status != ProjectStatus.ReceivingProposals && project.Status != ProjectStatus.InNegotiation)
            throw new InvalidOperationException("Este projeto não está aberto para propostas.");

        if (project.ClientId == freelancerId)
            throw new InvalidOperationException("Você não pode enviar proposta para seu próprio projeto.");

        var alreadyProposed = await proposalRepository.HasFreelancerProposedAsync(freelancerId, request.ProjectId);
        if (alreadyProposed)
            throw new InvalidOperationException("Você já enviou uma proposta para este projeto.");

        var proposal = request.ToEntity(freelancerId);
        await proposalRepository.AddAsync(proposal);
        await unitOfWork.SaveChangesAsync();

        var created = await proposalRepository.GetProposalWithDetailsAsync(proposal.Id)
            ?? throw new InvalidOperationException("Erro ao criar proposta.");

        await notificationService.CreateAsync(
            project.ClientId, "proposal_received",
            "Nova proposta recebida",
            $"{created.Freelancer.FullName} enviou uma proposta de R$ {request.Amount:N0} para \"{project.Title}\"",
            $"/projects/{project.Id}");

        await mailer.ProposalReceivedAsync(project.ClientId, project.Title, created.Freelancer.FullName, project.Id, WebBaseUrl);

        return created.ToDto();
    }

    public async Task<IEnumerable<ProposalDto>> GetByProjectIdAsync(Guid projectId, string viewerId)
    {
        var project = await projectRepository.GetByIdAsync(projectId)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        var proposals = await proposalRepository.GetProposalsByProjectIdAsync(projectId);

        // Cliente dono do projeto vê todas. Freelancer só vê a própria.
        if (project.ClientId == viewerId)
            return proposals.Select(p => p.ToDto());

        return proposals
            .Where(p => p.FreelancerId == viewerId)
            .Select(p => p.ToDto());
    }

    public async Task<IEnumerable<ProposalDto>> GetByFreelancerIdAsync(string freelancerId)
    {
        var proposals = await proposalRepository.GetProposalsByFreelancerIdAsync(freelancerId);
        return proposals.Select(p => p.ToDto());
    }

    public async Task<ProposalDto> AcceptAsync(Guid proposalId, string clientId)
    {
        const int maxAttempts = 3;
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await AcceptInternalAsync(proposalId, clientId);
            }
            catch (DbUpdateConcurrencyException) when (attempt < maxAttempts)
            {
                // Outra requisição contratou esta proposta (ou alterou o projeto) entre o load
                // e o save. Recarrega tudo e tenta de novo. Na 3ª tentativa devolve mensagem clara.
                await Task.Delay(50 * attempt);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new InvalidOperationException(
                    "Esta proposta foi alterada em outra operação. Recarregue a página e tente novamente.");
            }
        }
    }

    private async Task<ProposalDto> AcceptInternalAsync(Guid proposalId, string clientId)
    {
        var proposal = await proposalRepository.GetProposalWithDetailsAsync(proposalId)
            ?? throw new KeyNotFoundException("Proposta não encontrada.");

        if (proposal.Project.ClientId != clientId)
            throw new UnauthorizedAccessException("Você não tem permissão para aceitar esta proposta.");

        if (proposal.Status != ProposalStatus.Pending)
            throw new InvalidOperationException("Esta proposta não está pendente.");

        if (proposal.Project.Status != ProjectStatus.ReceivingProposals
            && proposal.Project.Status != ProjectStatus.InNegotiation)
            throw new InvalidOperationException("Este projeto não está mais disponível para contratação.");

        var fees = await platformFeeService.GetCurrentAsync();

        await using var tx = await unitOfWork.BeginTransactionAsync();
        try
        {
            proposal.Status = ProposalStatus.Accepted;
            proposal.Project.Status = ProjectStatus.Contracted;
            proposal.Project.AcceptedFreelancerId = proposal.FreelancerId;

            var otherProposals = await proposalRepository.GetProposalsByProjectIdAsync(proposal.ProjectId);
            foreach (var other in otherProposals.Where(p => p.Id != proposalId && p.Status == ProposalStatus.Pending))
            {
                // Regra do produto: propostas perdedoras NÃO são rejeitadas — ficam encerradas por contratação.
                other.Status = ProposalStatus.ClosedByContract;
                proposalRepository.Update(other);
            }

            var existingConversation = await conversationRepository
                .GetByProjectAndUsersAsync(proposal.ProjectId, clientId, proposal.FreelancerId);

            if (existingConversation is null)
            {
                var conversation = new Conversation
                {
                    ProjectId = proposal.ProjectId,
                    ClientId = clientId,
                    FreelancerId = proposal.FreelancerId
                };
                await conversationRepository.AddAsync(conversation);
            }

            var commission = Math.Round(proposal.Amount * (fees.CommissionPct / 100m), 2);
            var net = Math.Max(0, proposal.Amount - commission - fees.FreelancerFee);
            var revenue = fees.ClientFee + fees.FreelancerFee + commission;

            var payment = new Payment
            {
                ProjectId = proposal.ProjectId,
                ProposalId = proposal.Id,
                ClientId = clientId,
                FreelancerId = proposal.FreelancerId,
                GrossAmount = proposal.Amount,
                ClientFee = fees.ClientFee,
                FreelancerFee = fees.FreelancerFee,
                CommissionPct = fees.CommissionPct,
                CommissionAmount = commission,
                NetToFreelancer = net,
                PlatformRevenue = revenue,
                Status = PaymentStatus.Pending,
                PlatformFeeId = fees.Id == Guid.Empty ? null : fees.Id
            };
            await paymentRepository.AddAsync(payment);

            proposalRepository.Update(proposal);
            await unitOfWork.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }

        await notificationService.CreateAsync(
            proposal.FreelancerId, "proposal_accepted",
            "Proposta aceita!",
            $"Sua proposta para \"{proposal.Project.Title}\" foi aceita! Acesse o chat para combinar os detalhes.",
            $"/projects/{proposal.ProjectId}");

        await mailer.ProposalAcceptedAsync(proposal.FreelancerId, proposal.Project.Title, proposal.ProjectId, WebBaseUrl);

        return proposal.ToDto();
    }

    public async Task<ProposalDto> RejectAsync(Guid proposalId, string clientId)
    {
        var proposal = await proposalRepository.GetProposalWithDetailsAsync(proposalId)
            ?? throw new KeyNotFoundException("Proposta não encontrada.");

        if (proposal.Project.ClientId != clientId)
            throw new UnauthorizedAccessException("Você não tem permissão para rejeitar esta proposta.");

        if (proposal.Status != ProposalStatus.Pending)
            throw new InvalidOperationException("Esta proposta não está pendente.");

        proposal.Status = ProposalStatus.Rejected;
        proposalRepository.Update(proposal);
        await unitOfWork.SaveChangesAsync();

        await notificationService.CreateAsync(
            proposal.FreelancerId, "proposal_rejected",
            "Proposta não selecionada",
            $"Sua proposta para \"{proposal.Project.Title}\" não foi selecionada desta vez.",
            $"/proposals/my");

        await mailer.ProposalRejectedAsync(proposal.FreelancerId, proposal.Project.Title, WebBaseUrl);

        return proposal.ToDto();
    }

    public async Task WithdrawAsync(Guid proposalId, string freelancerId)
    {
        var proposal = await proposalRepository.GetByIdAsync(proposalId)
            ?? throw new KeyNotFoundException("Proposta não encontrada.");

        if (proposal.FreelancerId != freelancerId)
            throw new UnauthorizedAccessException("Você não tem permissão para retirar esta proposta.");

        if (proposal.Status != ProposalStatus.Pending)
            throw new InvalidOperationException("Esta proposta não está pendente.");

        proposal.Status = ProposalStatus.Withdrawn;
        proposalRepository.Update(proposal);
        await unitOfWork.SaveChangesAsync();
    }
}
