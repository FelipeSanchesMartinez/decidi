using Decidi.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Decidi.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? AvatarUrl { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? TermsAcceptedAt { get; set; }
    public string? TermsVersion { get; set; }

    public FreelancerProfile? FreelancerProfile { get; set; }
    public ICollection<Project> ClientProjects { get; set; } = [];
    public ICollection<Proposal> Proposals { get; set; } = [];
}
