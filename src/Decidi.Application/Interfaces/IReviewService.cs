using Decidi.Application.DTOs.Reviews;

namespace Decidi.Application.Interfaces;

public interface IReviewService
{
    Task<ReviewDto> CreateAsync(CreateReviewRequest request, string clientId);
    Task<ReviewDto> CreateFreelancerReviewAsync(CreateFreelancerReviewRequest request, string freelancerId);
    Task<IEnumerable<ReviewDto>> GetByFreelancerIdAsync(string freelancerId);
    Task<IEnumerable<ReviewDto>> GetByClientIdAsync(string clientId);
    Task<IEnumerable<PendingReviewDto>> GetPendingForUserAsync(string userId);
}
