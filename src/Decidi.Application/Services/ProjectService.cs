using Decidi.Application.DTOs.Common;
using Decidi.Application.DTOs.Projects;
using Decidi.Application.Interfaces;
using Decidi.Application.Mappings;
using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Decidi.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Decidi.Application.Services;

public class ProjectService(
    IProjectRepository projectRepository,
    ISkillRepository skillRepository,
    INotificationService notificationService,
    IMarketplaceMailer mailer,
    ISanitizer sanitizer,
    IConfiguration configuration,
    IUnitOfWork unitOfWork) : IProjectService
{
    public async Task<ProjectDto> CreateAsync(CreateProjectRequest request, string clientId)
    {
        request.Title = sanitizer.Sanitize(request.Title);
        request.Description = sanitizer.Sanitize(request.Description);
        var project = request.ToEntity(clientId);

        // Resolve skills from DB
        if (request.RequiredSkills.Count > 0)
        {
            var existingSkills = (await skillRepository.GetByNamesAsync(request.RequiredSkills)).ToList();
            var existingNames = existingSkills.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var skill in existingSkills)
                project.RequiredSkills.Add(skill);

            // Create new skills for custom entries ("Outra")
            foreach (var skillName in request.RequiredSkills.Where(n => !existingNames.Contains(n)))
            {
                var newSkill = new Skill { Name = skillName, Group = "Outros" };
                await skillRepository.AddAsync(newSkill);
                project.RequiredSkills.Add(newSkill);
            }
        }

        await projectRepository.AddAsync(project);
        await unitOfWork.SaveChangesAsync();

        var created = await projectRepository.GetProjectWithDetailsAsync(project.Id)
            ?? throw new InvalidOperationException("Erro ao criar projeto.");

        return created.ToDto();
    }

    public async Task<ProjectDto> GetByIdAsync(Guid id)
    {
        var project = await projectRepository.GetProjectWithDetailsAsync(id)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        return project.ToDto();
    }

    public async Task<PagedResult<ProjectListDto>> SearchAsync(ProjectSearchParams searchParams)
    {
        var projects = await projectRepository.GetProjectsWithDetailsAsync(
            searchParams.Search,
            searchParams.CategoryId,
            searchParams.Status,
            searchParams.BudgetMin,
            searchParams.BudgetMax,
            searchParams.BudgetType,
            searchParams.Skill,
            searchParams.SortBy,
            searchParams.Page,
            searchParams.PageSize);

        var totalCount = await projectRepository.CountAsync(
            searchParams.Search,
            searchParams.CategoryId,
            searchParams.Status,
            searchParams.BudgetMin,
            searchParams.BudgetMax,
            searchParams.BudgetType,
            searchParams.Skill);

        return new PagedResult<ProjectListDto>
        {
            Items = projects.Select(p => p.ToListDto()),
            TotalCount = totalCount,
            Page = searchParams.Page,
            PageSize = searchParams.PageSize
        };
    }

    public async Task<ProjectDto> UpdateAsync(Guid id, UpdateProjectRequest request, string clientId)
    {
        var project = await projectRepository.GetProjectWithDetailsAsync(id)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        if (project.ClientId != clientId)
            throw new UnauthorizedAccessException("Você não tem permissão para editar este projeto.");

        project.Title = sanitizer.Sanitize(request.Title);
        project.Description = sanitizer.Sanitize(request.Description);
        project.BudgetMin = request.BudgetMin;
        project.BudgetMax = request.BudgetMax;
        project.BudgetType = request.BudgetType;
        project.Deadline = request.Deadline;
        project.CategoryId = request.CategoryId;
        // Status não é mutável via UpdateAsync — usa endpoints específicos (complete, cancel).

        projectRepository.Update(project);
        await unitOfWork.SaveChangesAsync();

        return project.ToDto();
    }

    public async Task DeleteAsync(Guid id, string clientId)
    {
        var project = await projectRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        if (project.ClientId != clientId)
            throw new UnauthorizedAccessException("Você não tem permissão para excluir este projeto.");

        projectRepository.Remove(project);
        await unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProjectListDto>> GetByClientIdAsync(string clientId)
    {
        var projects = await projectRepository.GetProjectsByClientIdAsync(clientId);
        return projects.Select(p => p.ToListDto());
    }

    public async Task<ProjectDto> StartExecutionAsync(Guid id, string clientId)
    {
        var project = await projectRepository.GetProjectWithDetailsAsync(id)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        if (project.ClientId != clientId)
            throw new UnauthorizedAccessException("Apenas o dono do projeto pode iniciar a execução.");

        if (project.Status != ProjectStatus.Contracted)
            throw new InvalidOperationException("Só é possível iniciar execução de projetos contratados.");

        project.Status = ProjectStatus.InProgress;
        projectRepository.Update(project);
        await unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(project.AcceptedFreelancerId))
            await notificationService.CreateAsync(
                project.AcceptedFreelancerId, "project_started",
                "Execução iniciada",
                $"O cliente iniciou a execução do projeto \"{project.Title}\".",
                $"/projects/{project.Id}");

        return project.ToDto();
    }

    public async Task<ProjectDto> CompleteAsync(Guid id, string clientId)
    {
        var project = await projectRepository.GetProjectWithDetailsAsync(id)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        if (project.ClientId != clientId)
            throw new UnauthorizedAccessException("Apenas o dono do projeto pode concluir.");

        if (project.Status != ProjectStatus.InProgress && project.Status != ProjectStatus.Contracted)
            throw new InvalidOperationException("Só é possível concluir projetos em execução ou contratados.");

        project.Status = ProjectStatus.Completed;
        projectRepository.Update(project);
        await unitOfWork.SaveChangesAsync();

        if (!string.IsNullOrEmpty(project.AcceptedFreelancerId))
            await notificationService.CreateAsync(
                project.AcceptedFreelancerId, "project_completed",
                "Projeto concluído",
                $"O projeto \"{project.Title}\" foi concluído. Você já pode avaliar o cliente.",
                $"/projects/{project.Id}");

        // Lembrete para o cliente avaliar o profissional.
        await notificationService.CreateAsync(
            project.ClientId, "review_pending",
            "Avalie o profissional",
            $"Seu projeto \"{project.Title}\" foi concluído. Que tal avaliar a experiência?",
            $"/projects/{project.Id}");

        var webBaseUrl = configuration["WebBaseUrl"] ?? "https://decidi.com.br";
        if (!string.IsNullOrEmpty(project.AcceptedFreelancerId))
            await mailer.ProjectCompletedAsync(project.AcceptedFreelancerId, project.Title, project.Id, webBaseUrl);
        await mailer.ProjectCompletedAsync(project.ClientId, project.Title, project.Id, webBaseUrl);

        return project.ToDto();
    }

    public async Task<ProjectDto> CancelAsync(Guid id, string clientId, string? reason)
    {
        var project = await projectRepository.GetProjectWithDetailsAsync(id)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        if (project.ClientId != clientId)
            throw new UnauthorizedAccessException("Apenas o dono do projeto pode cancelar.");

        if (project.Status == ProjectStatus.Completed || project.Status == ProjectStatus.Cancelled)
            throw new InvalidOperationException("Projeto já está finalizado.");

        project.Status = ProjectStatus.Cancelled;
        projectRepository.Update(project);
        await unitOfWork.SaveChangesAsync();

        var reasonText = string.IsNullOrWhiteSpace(reason)
            ? string.Empty
            : $" Motivo: {sanitizer.Sanitize(reason)}";

        if (!string.IsNullOrEmpty(project.AcceptedFreelancerId))
            await notificationService.CreateAsync(
                project.AcceptedFreelancerId, "project_cancelled",
                "Projeto cancelado",
                $"O projeto \"{project.Title}\" foi cancelado pelo cliente.{reasonText}",
                $"/projects/{project.Id}");

        return project.ToDto();
    }
}
