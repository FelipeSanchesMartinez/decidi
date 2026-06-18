using Decidi.Application.DTOs.Reviews;
using Decidi.Domain.Entities;

namespace Decidi.Application.Mappings;

public static class ReviewMappings
{
    public static ReviewDto ToDto(this Review review) => new()
    {
        Id = review.Id,
        Rating = review.Rating,
        RatingQuality = review.RatingQuality,
        RatingCommunication = review.RatingCommunication,
        RatingDeadline = review.RatingDeadline,
        Comment = review.Comment,
        ProjectId = review.ProjectId,
        ProjectTitle = review.Project?.Title ?? string.Empty,
        ClientId = review.ClientId,
        ClientName = review.Client?.FullName ?? string.Empty,
        FreelancerId = review.FreelancerId,
        FreelancerName = review.Freelancer?.FullName ?? string.Empty,
        ReviewerRole = review.ReviewerRole,
        Visibility = review.Visibility,
        ReleasedAt = review.ReleasedAt,
        CreatedAt = review.CreatedAt
    };
}
