using Decidi.Application.DTOs.Common;
using Decidi.Application.Interfaces;
using Decidi.Application.Mappings;
using Decidi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Decidi.Application.Services;

public class SkillService(
    ISkillRepository skillRepository,
    IMemoryCache cache) : ISkillService
{
    private const string AllCacheKey = "skills:all";
    private static readonly TimeSpan AllTtl = TimeSpan.FromMinutes(30);

    public async Task<IEnumerable<SkillDto>> GetAllAsync()
    {
        if (cache.TryGetValue<IEnumerable<SkillDto>>(AllCacheKey, out var cached) && cached is not null)
            return cached;

        var skills = await skillRepository.GetAllGroupedAsync();
        var dtos = skills.Select(s => s.ToDto()).ToList();
        cache.Set(AllCacheKey, (IEnumerable<SkillDto>)dtos, AllTtl);
        return dtos;
    }
}
