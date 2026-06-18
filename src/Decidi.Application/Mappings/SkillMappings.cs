using Decidi.Application.DTOs.Common;
using Decidi.Domain.Entities;

namespace Decidi.Application.Mappings;

public static class SkillMappings
{
    public static SkillDto ToDto(this Skill skill) => new()
    {
        Id = skill.Id,
        Name = skill.Name,
        Group = skill.Group
    };
}
