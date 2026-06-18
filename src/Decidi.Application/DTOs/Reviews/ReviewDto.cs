using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Reviews;

public class ReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public int? RatingQuality { get; set; }
    public int? RatingCommunication { get; set; }
    public int? RatingDeadline { get; set; }
    public string? Comment { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string FreelancerId { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public ReviewerRole ReviewerRole { get; set; }
    public ReviewVisibility Visibility { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PendingReviewDto
{
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string CounterpartyId { get; set; } = string.Empty;
    public string CounterpartyName { get; set; } = string.Empty;
    public string? CounterpartyAvatarUrl { get; set; }
    public DateTime ProjectCompletedAt { get; set; }
    public ReviewerRole AsRole { get; set; }
}
