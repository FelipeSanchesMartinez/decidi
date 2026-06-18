using Decidi.Domain.Common;
using Decidi.Domain.Enums;

namespace Decidi.Domain.Entities;

public class Review : BaseEntity
{
    /// <summary>Nota geral 1-5 (compat — pode ser média dos critérios).</summary>
    public int Rating { get; set; }

    /// <summary>Critério opcional: qualidade do trabalho (1-5).</summary>
    public int? RatingQuality { get; set; }

    /// <summary>Critério opcional: comunicação (1-5).</summary>
    public int? RatingCommunication { get; set; }

    /// <summary>Critério opcional: cumprimento de prazo (1-5).</summary>
    public int? RatingDeadline { get; set; }

    public string? Comment { get; set; }

    /// <summary>Quem foi o autor (Client → avalia Freelancer, Freelancer → avalia Client).</summary>
    public ReviewerRole ReviewerRole { get; set; } = ReviewerRole.Client;

    /// <summary>Blind review: Pending até o par-companheiro avaliar OU 14 dias.</summary>
    public ReviewVisibility Visibility { get; set; } = ReviewVisibility.Pending;

    /// <summary>Momento em que a review foi liberada publicamente.</summary>
    public DateTime? ReleasedAt { get; set; }

    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string ClientId { get; set; } = string.Empty;
    public ApplicationUser Client { get; set; } = null!;

    public string FreelancerId { get; set; } = string.Empty;
    public ApplicationUser Freelancer { get; set; } = null!;
}
