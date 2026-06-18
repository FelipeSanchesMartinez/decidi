using Decidi.Domain.Common;

namespace Decidi.Domain.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;

    public ICollection<FreelancerProfile> FreelancerProfiles { get; set; } = [];
    public ICollection<Project> Projects { get; set; } = [];
}
