using Decidi.Application.DTOs.Payments;
using Decidi.Application.Interfaces;
using Decidi.Application.Mappings;
using Decidi.Domain.Enums;
using Decidi.Domain.Interfaces;

namespace Decidi.Application.Services;

public class MilestoneService(
    IMilestoneRepository milestoneRepository,
    IProjectRepository projectRepository,
    IUnitOfWork unitOfWork) : IMilestoneService
{
    public async Task<MilestoneDto> CreateAsync(CreateMilestoneRequest request, string clientId)
    {
        var project = await projectRepository.GetByIdAsync(request.ProjectId)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        if (project.ClientId != clientId)
            throw new UnauthorizedAccessException("Apenas o cliente pode criar milestones.");

        var milestone = request.ToEntity();
        await milestoneRepository.AddAsync(milestone);
        await unitOfWork.SaveChangesAsync();

        return milestone.ToDto();
    }

    public async Task<IEnumerable<MilestoneDto>> GetByProjectIdAsync(Guid projectId)
    {
        var milestones = await milestoneRepository.GetByProjectIdAsync(projectId);
        return milestones.Select(m => m.ToDto());
    }

    public async Task<MilestoneDto> UpdateStatusAsync(Guid milestoneId, UpdateMilestoneStatusRequest request, string userId)
    {
        var milestone = await milestoneRepository.GetByIdAsync(milestoneId)
            ?? throw new KeyNotFoundException("Milestone não encontrado.");

        var project = await projectRepository.GetByIdAsync(milestone.ProjectId)
            ?? throw new KeyNotFoundException("Projeto não encontrado.");

        var isClient = project.ClientId == userId;
        var isFreelancer = project.AcceptedFreelancerId == userId;

        if (!isClient && !isFreelancer)
            throw new UnauthorizedAccessException("Você não tem permissão para alterar este milestone.");

        ValidateStatusTransition(milestone.Status, request.NewStatus, isClient);

        milestone.Status = request.NewStatus;

        if (request.NewStatus == MilestoneStatus.Paid)
            milestone.PaidAt = DateTime.UtcNow;

        milestoneRepository.Update(milestone);
        await unitOfWork.SaveChangesAsync();

        return milestone.ToDto();
    }

    private static void ValidateStatusTransition(MilestoneStatus current, MilestoneStatus next, bool isClient)
    {
        var valid = (current, next, isClient) switch
        {
            (MilestoneStatus.Pending, MilestoneStatus.InProgress, false) => true,
            (MilestoneStatus.InProgress, MilestoneStatus.Submitted, false) => true,
            (MilestoneStatus.Submitted, MilestoneStatus.Approved, true) => true,
            (MilestoneStatus.Approved, MilestoneStatus.Paid, true) => true,
            (MilestoneStatus.Submitted, MilestoneStatus.InProgress, true) => true, // reject back
            _ => false
        };

        if (!valid)
            throw new InvalidOperationException($"Transição de status inválida: {current} -> {next}.");
    }
}
