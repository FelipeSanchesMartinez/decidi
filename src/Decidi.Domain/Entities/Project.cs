using Decidi.Domain.Common;
using Decidi.Domain.Enums;

namespace Decidi.Domain.Entities;

public class Project : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    // Cliente NÃO informa valor. Estes campos ficam opcionais — preenchidos só se o cliente
    // quiser sinalizar uma faixa de referência (não aparece como obrigatório no fluxo).
    public decimal? BudgetMin { get; set; }
    public decimal? BudgetMax { get; set; }
    public ProjectBudgetType? BudgetType { get; set; }
    public ProjectStatus Status { get; set; } = ProjectStatus.ReceivingProposals;
    public DateTime? Deadline { get; set; }

    public string ClientId { get; set; } = string.Empty;
    public ApplicationUser Client { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string? AcceptedFreelancerId { get; set; }
    public ApplicationUser? AcceptedFreelancer { get; set; }

    public ICollection<Proposal> Proposals { get; set; } = [];
    public ICollection<Skill> RequiredSkills { get; set; } = [];
    public ICollection<Milestone> Milestones { get; set; } = [];
    public Conversation? Conversation { get; set; }

    /// <summary>Concurrency token — atualizado a cada SaveChanges para detectar update concorrente.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
