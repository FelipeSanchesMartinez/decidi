using Decidi.Application.DTOs.Common;
using Decidi.Domain.Entities;

namespace Decidi.Application.Mappings;

public static class CategoryMappings
{
    public static CategoryDto ToDto(this Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Slug = category.Slug,
        Description = category.Description
    };
}
