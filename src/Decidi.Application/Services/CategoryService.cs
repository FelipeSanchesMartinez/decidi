using Decidi.Application.DTOs.Common;
using Decidi.Application.Interfaces;
using Decidi.Application.Mappings;
using Decidi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Decidi.Application.Services;

public class CategoryService(
    ICategoryRepository categoryRepository,
    IMemoryCache cache) : ICategoryService
{
    private const string AllCacheKey = "categories:all";
    private static readonly TimeSpan AllTtl = TimeSpan.FromHours(1);

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        if (cache.TryGetValue<IEnumerable<CategoryDto>>(AllCacheKey, out var cached) && cached is not null)
            return cached;

        var categories = await categoryRepository.GetAllAsync();
        var dtos = categories.Select(c => c.ToDto()).ToList();
        cache.Set(AllCacheKey, (IEnumerable<CategoryDto>)dtos, AllTtl);
        return dtos;
    }

    public async Task<CategoryDto?> GetBySlugAsync(string slug)
    {
        // Normaliza ANTES de consultar — slugs no DB são lowercase (seed), mas a URL
        // pode chegar com qualquer capitalização. Sem isso a chave e a query divergem
        // e cacheamos null indevidamente.
        var normalized = (slug ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(normalized)) return null;

        var key = $"category:slug:{normalized}";
        if (cache.TryGetValue<CategoryDto?>(key, out var cached))
            return cached;

        var category = await categoryRepository.GetBySlugAsync(normalized);
        var dto = category?.ToDto();
        cache.Set(key, dto, AllTtl);
        return dto;
    }
}
