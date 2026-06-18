using Decidi.Application.DTOs.Auth;
using Decidi.Domain.Entities;

namespace Decidi.Application.Mappings;

public static class UserMappings
{
    public static UserProfileDto ToProfileDto(this ApplicationUser user)
    {
        var dto = new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = user.Role,
            AvatarUrl = user.AvatarUrl,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            CreatedAt = user.CreatedAt
        };

        if (user.FreelancerProfile is not null)
        {
            dto.Title = user.FreelancerProfile.Title;
            dto.Bio = user.FreelancerProfile.Bio;
            dto.HourlyRate = user.FreelancerProfile.HourlyRate;
            dto.PortfolioUrl = user.FreelancerProfile.PortfolioUrl;
            dto.Skills = user.FreelancerProfile.Skills.Select(s => s.Name).ToList();
        }

        return dto;
    }
}
