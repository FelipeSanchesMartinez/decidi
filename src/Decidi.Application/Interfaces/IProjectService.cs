using Decidi.Application.DTOs.Common;
using Decidi.Application.DTOs.Projects;

namespace Decidi.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto> CreateAsync(CreateProjectRequest request, string clientId);
    Task<ProjectDto> GetByIdAsync(Guid id);
    Task<PagedResult<ProjectListDto>> SearchAsync(ProjectSearchParams searchParams);
    Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectRequest request, string clientId);
    Task DeleteAsync(Guid id, string clientId);
    Task<IEnumerable<ProjectListDto>> GetByClientIdAsync(string clientId);
    Task<ProjectDto> StartExecutionAsync(Guid id, string clientId);
    Task<ProjectDto> CompleteAsync(Guid id, string clientId);
    Task<ProjectDto> CancelAsync(Guid id, string clientId, string? reason);
}
