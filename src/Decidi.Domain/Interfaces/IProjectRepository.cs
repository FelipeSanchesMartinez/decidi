using Decidi.Domain.Entities;
using Decidi.Domain.Enums;

namespace Decidi.Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    Task<IEnumerable<Project>> GetProjectsWithDetailsAsync(
        string? search = null,
        Guid? categoryId = null,
        ProjectStatus? status = null,
        decimal? budgetMin = null,
        decimal? budgetMax = null,
        ProjectBudgetType? budgetType = null,
        string? skill = null,
        string? sortBy = null,
        int page = 1,
        int pageSize = 10);

    Task<Project?> GetProjectWithDetailsAsync(Guid id);
    Task<IEnumerable<Project>> GetProjectsByClientIdAsync(string clientId);
    /// <summary>Projetos concluídos onde o usuário foi Cliente OU Freelancer aceito. Sem paginação.</summary>
    Task<IEnumerable<Project>> GetCompletedForUserAsync(string userId);

    Task<int> CountAsync(
        string? search = null,
        Guid? categoryId = null,
        ProjectStatus? status = null,
        decimal? budgetMin = null,
        decimal? budgetMax = null,
        ProjectBudgetType? budgetType = null,
        string? skill = null);
}
