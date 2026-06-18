using Decidi.Domain.Entities;
using Decidi.Domain.Enums;

namespace Decidi.Domain.Interfaces;

public interface IReviewRepository : IRepository<Review>
{
    Task<IEnumerable<Review>> GetByFreelancerIdAsync(string freelancerId, bool onlyReleased = true);
    Task<IEnumerable<Review>> GetByClientIdAsync(string clientId, bool onlyReleased = true);
    Task<Review?> GetByProjectAndReviewerAsync(Guid projectId, ReviewerRole reviewerRole);
    /// <summary>Média de reviews recebidas por este usuário, filtrando quem avaliou.</summary>
    Task<double> GetAverageRatingAsync(string userId, ReviewerRole reviewerRole, bool onlyReleased = true);
    Task<int> GetCountAsync(string userId, ReviewerRole reviewerRole, bool onlyReleased = true);
    /// <summary>Atualiza várias reviews em lote (usado para release).</summary>
    void UpdateRange(IEnumerable<Review> reviews);

    /// <summary>Reviews Pending do usuário cuja janela de blind review expirou (CreatedAt &lt;= threshold).</summary>
    Task<IEnumerable<Review>> GetPendingExpiredForUserAsync(string userId, DateTime threshold);

    /// <summary>Reviews existentes em projetos específicos para um papel — para checagem em batch.</summary>
    Task<IEnumerable<Review>> GetByProjectsAndReviewerAsync(IEnumerable<Guid> projectIds, ReviewerRole reviewerRole);
}
