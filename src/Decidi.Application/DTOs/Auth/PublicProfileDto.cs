using Decidi.Application.DTOs.Common;
using Decidi.Application.DTOs.Reviews;

namespace Decidi.Application.DTOs.Auth;

public class PublicProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public string? Title { get; set; }
    public string? Bio { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? PortfolioUrl { get; set; }

    public List<SkillDto> Skills { get; set; } = [];
    public List<ReviewDto> Reviews { get; set; } = [];
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int CompletedProjects { get; set; }
}
