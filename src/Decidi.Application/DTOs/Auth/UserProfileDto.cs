using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Auth;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }

    // Freelancer-specific
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? PortfolioUrl { get; set; }
    public List<string> Skills { get; set; } = [];
}
