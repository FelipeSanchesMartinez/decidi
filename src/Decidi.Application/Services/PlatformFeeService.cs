using Decidi.Application.DTOs.Payments;
using Decidi.Application.Interfaces;
using Decidi.Domain.Entities;
using Decidi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Decidi.Application.Services;

public class PlatformFeeService(
    IRepository<PlatformFee> repository,
    IMemoryCache cache) : IPlatformFeeService
{
    private const string CacheKey = "platformfee:current";

    public async Task<PlatformFeeDto> GetCurrentAsync()
    {
        if (cache.TryGetValue<PlatformFeeDto>(CacheKey, out var cached) && cached is not null)
            return cached;

        var all = await repository.GetAllAsync();
        var current = all
            .Where(f => f.IsActive && f.EffectiveFrom <= DateTime.UtcNow)
            .OrderByDescending(f => f.EffectiveFrom)
            .FirstOrDefault();

        // Fallback caso o seed ainda não tenha rodado: valores padrão.
        var dto = current is null
            ? new PlatformFeeDto
            {
                Id = Guid.Empty,
                EffectiveFrom = DateTime.UtcNow,
                ClientFee = 3.99m,
                FreelancerFee = 2.99m,
                CommissionPct = 12m
            }
            : new PlatformFeeDto
            {
                Id = current.Id,
                EffectiveFrom = current.EffectiveFrom,
                ClientFee = current.ClientFee,
                FreelancerFee = current.FreelancerFee,
                CommissionPct = current.CommissionPct
            };

        cache.Set(CacheKey, dto, TimeSpan.FromMinutes(5));
        return dto;
    }
}
