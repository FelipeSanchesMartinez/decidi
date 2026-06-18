using Decidi.Domain.Common;

namespace Decidi.Domain.Entities;

public class FreelancerProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public string? PortfolioUrl { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Skill> Skills { get; set; } = [];
}
