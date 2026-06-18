using Decidi.Domain.Common;

namespace Decidi.Domain.Entities;

public class Conversation : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string ClientId { get; set; } = string.Empty;
    public ApplicationUser Client { get; set; } = null!;

    public string FreelancerId { get; set; } = string.Empty;
    public ApplicationUser Freelancer { get; set; } = null!;

    public ICollection<Message> Messages { get; set; } = [];
}
