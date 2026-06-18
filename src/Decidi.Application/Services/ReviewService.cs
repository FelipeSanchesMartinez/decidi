using Decidi.Application.DTOs.Reviews;
using Decidi.Application.Interfaces;
using Decidi.Application.Mappings;
using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Decidi.Domain.Interfaces;

namespace Decidi.Application.Services;

public class ReviewService(
    IReviewRepository reviewRepository,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork) : IReviewService
{
    /// <summary>Janela de blind review: após N dias sem o par avaliar, libera a review unilateralmente.</summary>
    private static readonly TimeSpan BlindWindow = TimeSpan.FromDays(14);

    public async Task<ReviewDto> CreateAsync(CreateReviewRequest request, string clientId)
    {
        var project = await projectRepository.GetProjectWithDetailsAsync(request.ProjectId)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        if (project.ClientId != clientId)
            throw new UnauthorizedAccessException("Apenas o cliente do projeto pode avaliar.");

        if (project.Status != ProjectStatus.Completed)
            throw new InvalidOperationException("O projeto precisa estar concluído para avaliar.");

        if (project.AcceptedFreelancerId != request.FreelancerId)
            throw new InvalidOperationException("Freelancer não corresponde ao projeto.");

        var existing = await reviewRepository.GetByProjectAndReviewerAsync(request.ProjectId, ReviewerRole.Client);
        if (existing is not null)
            throw new InvalidOperationException("Você já avaliou este projeto.");

        var review = new Review
        {
            Rating = request.Rating,
            RatingQuality = request.RatingQuality,
            RatingCommunication = request.RatingCommunication,
            RatingDeadline = request.RatingDeadline,
            Comment = request.Comment,
            ProjectId = request.ProjectId,
            ClientId = clientId,
            FreelancerId = request.FreelancerId,
            ReviewerRole = ReviewerRole.Client,
            Visibility = ReviewVisibility.Pending
        };

        await reviewRepository.AddAsync(review);
        await ReleaseIfCounterpartExistsAsync(review);
        await unitOfWork.SaveChangesAsync();

        return review.ToDto();
    }

    public async Task<ReviewDto> CreateFreelancerReviewAsync(CreateFreelancerReviewRequest request, string freelancerId)
    {
        var project = await projectRepository.GetProjectWithDetailsAsync(request.ProjectId)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        if (project.AcceptedFreelancerId != freelancerId)
            throw new UnauthorizedAccessException("Apenas o freelancer contratado pode avaliar o cliente.");

        if (project.Status != ProjectStatus.Completed)
            throw new InvalidOperationException("O projeto precisa estar concluído para avaliar.");

        var existing = await reviewRepository.GetByProjectAndReviewerAsync(request.ProjectId, ReviewerRole.Freelancer);
        if (existing is not null)
            throw new InvalidOperationException("Você já avaliou o cliente deste projeto.");

        var review = new Review
        {
            Rating = request.Rating,
            RatingQuality = request.RatingQuality,
            RatingCommunication = request.RatingCommunication,
            RatingDeadline = request.RatingDeadline,
            Comment = request.Comment,
            ProjectId = request.ProjectId,
            ClientId = project.ClientId,
            FreelancerId = freelancerId,
            ReviewerRole = ReviewerRole.Freelancer,
            Visibility = ReviewVisibility.Pending
        };

        await reviewRepository.AddAsync(review);
        await ReleaseIfCounterpartExistsAsync(review);
        await unitOfWork.SaveChangesAsync();

        return review.ToDto();
    }

    public async Task<IEnumerable<ReviewDto>> GetByFreelancerIdAsync(string freelancerId)
    {
        await LazyReleaseExpiredAsync(freelancerId);
        var reviews = await reviewRepository.GetByFreelancerIdAsync(freelancerId, onlyReleased: true);
        return reviews.Select(r => r.ToDto());
    }

    public async Task<IEnumerable<ReviewDto>> GetByClientIdAsync(string clientId)
    {
        await LazyReleaseExpiredAsync(clientId);
        var reviews = await reviewRepository.GetByClientIdAsync(clientId, onlyReleased: true);
        return reviews.Select(r => r.ToDto());
    }

    public async Task<IEnumerable<PendingReviewDto>> GetPendingForUserAsync(string userId)
    {
        // Sem paginação: traz só os projetos completados onde o user está envolvido (Client ou Freelancer aceito).
        var completedProjects = (await projectRepository.GetCompletedForUserAsync(userId)).ToList();
        if (completedProjects.Count == 0) return [];

        // Para cada projeto, o papel do user define a checagem.
        var clientProjectIds = completedProjects.Where(p => p.ClientId == userId).Select(p => p.Id).ToList();
        var freelancerProjectIds = completedProjects.Where(p => p.AcceptedFreelancerId == userId).Select(p => p.Id).ToList();

        // Busca em LOTE as reviews já feitas pelo user (1 query por papel envolvido).
        var alreadyReviewedAsClient = (await reviewRepository
            .GetByProjectsAndReviewerAsync(clientProjectIds, ReviewerRole.Client))
            .Select(r => r.ProjectId).ToHashSet();

        var alreadyReviewedAsFreelancer = (await reviewRepository
            .GetByProjectsAndReviewerAsync(freelancerProjectIds, ReviewerRole.Freelancer))
            .Select(r => r.ProjectId).ToHashSet();

        var result = new List<PendingReviewDto>();
        foreach (var p in completedProjects)
        {
            var isClient = p.ClientId == userId;
            var asRole = isClient ? ReviewerRole.Client : ReviewerRole.Freelancer;
            var reviewed = isClient
                ? alreadyReviewedAsClient.Contains(p.Id)
                : alreadyReviewedAsFreelancer.Contains(p.Id);
            if (reviewed) continue;

            var counterpartyId = isClient ? (p.AcceptedFreelancerId ?? string.Empty) : p.ClientId;
            var counterpartyName = isClient ? (p.AcceptedFreelancer?.FullName ?? string.Empty) : (p.Client?.FullName ?? string.Empty);
            var counterpartyAvatar = isClient ? p.AcceptedFreelancer?.AvatarUrl : p.Client?.AvatarUrl;

            result.Add(new PendingReviewDto
            {
                ProjectId = p.Id,
                ProjectTitle = p.Title,
                CounterpartyId = counterpartyId,
                CounterpartyName = counterpartyName,
                CounterpartyAvatarUrl = counterpartyAvatar,
                ProjectCompletedAt = p.UpdatedAt ?? p.CreatedAt,
                AsRole = asRole
            });
        }

        return result.OrderByDescending(p => p.ProjectCompletedAt);
    }

    /// <summary>
    /// Se o par-companheiro já existe (mesmo projeto, outro ReviewerRole), libera ambas
    /// imediatamente. Caso contrário, a nova review fica Pending até o outro lado avaliar
    /// ou a janela de 14 dias expirar.
    /// </summary>
    private async Task ReleaseIfCounterpartExistsAsync(Review newReview)
    {
        var counterRole = newReview.ReviewerRole == ReviewerRole.Client
            ? ReviewerRole.Freelancer
            : ReviewerRole.Client;
        var counter = await reviewRepository.GetByProjectAndReviewerAsync(newReview.ProjectId, counterRole);
        if (counter is null) return;

        var now = DateTime.UtcNow;
        newReview.Visibility = ReviewVisibility.Released;
        newReview.ReleasedAt = now;
        if (counter.Visibility == ReviewVisibility.Pending)
        {
            counter.Visibility = ReviewVisibility.Released;
            counter.ReleasedAt = now;
            reviewRepository.UpdateRange([counter]);
        }
    }

    /// <summary>
    /// Lazy release: ao ler reviews de um usuário, marca como Released as Pending com
    /// CreatedAt + 14 dias <= agora (janela de blind review expirada). Sem worker dedicado.
    /// </summary>
    private async Task LazyReleaseExpiredAsync(string userId)
    {
        var threshold = DateTime.UtcNow - BlindWindow;
        var toRelease = (await reviewRepository.GetPendingExpiredForUserAsync(userId, threshold)).ToList();
        if (toRelease.Count == 0) return;

        var now = DateTime.UtcNow;
        foreach (var r in toRelease)
        {
            r.Visibility = ReviewVisibility.Released;
            r.ReleasedAt = now;
        }
        reviewRepository.UpdateRange(toRelease);
        await unitOfWork.SaveChangesAsync();
    }
}
