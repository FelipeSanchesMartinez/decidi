using Decidi.Application.DTOs.Common;

namespace Decidi.Application.Interfaces;

public interface ISkillService
{
    Task<IEnumerable<SkillDto>> GetAllAsync();
}
