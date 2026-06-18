using Decidi.Domain.Common;

namespace Decidi.Domain.Entities;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Project> Projects { get; set; } = [];
}
