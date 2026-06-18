using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Decidi.Domain.Interfaces;
using Decidi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Decidi.Infrastructure.Repositories;

public class ProjectRepository(AppDbContext context) : Repository<Project>(context), IProjectRepository
{
    public async Task<IEnumerable<Project>> GetProjectsWithDetailsAsync(
        string? search = null,
        Guid? categoryId = null,
        ProjectStatus? status = null,
        decimal? budgetMin = null,
        decimal? budgetMax = null,
        ProjectBudgetType? budgetType = null,
        string? skill = null,
        string? sortBy = null,
        int page = 1,
        int pageSize = 10)
    {
        var query = BuildFilteredQuery(search, categoryId, status, budgetMin, budgetMax, budgetType, skill);

        query = sortBy switch
        {
            "budget_asc" => query.OrderBy(p => p.BudgetMax),
            "budget_desc" => query.OrderByDescending(p => p.BudgetMax),
            "proposals" => query.OrderByDescending(p => p.Proposals.Count),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        return await query
            .Include(p => p.Client)
            .Include(p => p.Category)
            .Include(p => p.RequiredSkills)
            .Include(p => p.Proposals)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Project?> GetProjectWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Client)
            .Include(p => p.Category)
            .Include(p => p.RequiredSkills)
            .Include(p => p.Proposals)
            .Include(p => p.AcceptedFreelancer)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Project>> GetProjectsByClientIdAsync(string clientId)
    {
        return await _dbSet
            .Where(p => p.ClientId == clientId)
            .Include(p => p.Client)
            .Include(p => p.Category)
            .Include(p => p.RequiredSkills)
            .Include(p => p.Proposals)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Project>> GetCompletedForUserAsync(string userId)
    {
        return await _dbSet
            .Where(p => p.Status == ProjectStatus.Completed
                && (p.ClientId == userId || p.AcceptedFreelancerId == userId))
            .Include(p => p.Client)
            .Include(p => p.AcceptedFreelancer)
            .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<int> CountAsync(
        string? search = null,
        Guid? categoryId = null,
        ProjectStatus? status = null,
        decimal? budgetMin = null,
        decimal? budgetMax = null,
        ProjectBudgetType? budgetType = null,
        string? skill = null)
    {
        return await BuildFilteredQuery(search, categoryId, status, budgetMin, budgetMax, budgetType, skill).CountAsync();
    }

    private IQueryable<Project> BuildFilteredQuery(
        string? search, Guid? categoryId, ProjectStatus? status,
        decimal? budgetMin = null, decimal? budgetMax = null,
        ProjectBudgetType? budgetType = null, string? skill = null)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Title.Contains(search) || p.Description.Contains(search));

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (budgetMin.HasValue)
            query = query.Where(p => p.BudgetMax >= budgetMin.Value);

        if (budgetMax.HasValue)
            query = query.Where(p => p.BudgetMin <= budgetMax.Value);

        if (budgetType.HasValue)
            query = query.Where(p => p.BudgetType == budgetType.Value);

        if (!string.IsNullOrWhiteSpace(skill))
            query = query.Where(p => p.RequiredSkills.Any(s => s.Name == skill));

        return query;
    }
}
