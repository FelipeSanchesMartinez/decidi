using System.ComponentModel.DataAnnotations;

namespace Decidi.Web.Models;

public enum UserRole { Client = 0, Freelancer = 1 }
public enum ProjectStatus
{
    ReceivingProposals = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
    Draft = 4,
    InNegotiation = 5,
    Contracted = 6
}
public enum ProposalStatus { Pending = 0, Accepted = 1, Rejected = 2, Withdrawn = 3, ClosedByContract = 4 }
public enum MilestoneStatus { Pending = 0, InProgress = 1, Submitted = 2, Approved = 3, Paid = 4 }
public enum ProjectBudgetType { Fixed = 0, Hourly = 1 }
public enum ReviewerRole { Client = 0, Freelancer = 1 }
public enum ReviewVisibility { Pending = 0, Released = 1 }

public class RegisterRequest
{
    [Required(ErrorMessage = "Nome completo é obrigatório")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Senha deve ter no mínimo 6 caracteres")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de senha é obrigatória")]
    [Compare(nameof(Password), ErrorMessage = "Senhas não conferem")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; }

    public bool AcceptedTerms { get; set; }
}

public class LoginRequest
{
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Senha é obrigatória")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool RequiresEmailConfirmation { get; set; }
}

public class MessageResponse
{
    public string Message { get; set; } = string.Empty;
}

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string? AvatarUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? PortfolioUrl { get; set; }
    public List<string> Skills { get; set; } = [];
}

public class PublicProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Title { get; set; }
    public string? Bio { get; set; }
    public decimal? HourlyRate { get; set; }
    public string? PortfolioUrl { get; set; }
    public List<SkillDto> Skills { get; set; } = [];
    public List<ReviewDto> Reviews { get; set; } = [];
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int CompletedProjects { get; set; }
}

public class SkillDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}

public class ReviewDto
{
    public Guid Id { get; set; }
    public int Rating { get; set; }
    public int? RatingQuality { get; set; }
    public int? RatingCommunication { get; set; }
    public int? RatingDeadline { get; set; }
    public string? Comment { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string FreelancerId { get; set; } = string.Empty;
    public string FreelancerName { get; set; } = string.Empty;
    public ReviewerRole ReviewerRole { get; set; }
    public ReviewVisibility Visibility { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PendingReviewDto
{
    public Guid ProjectId { get; set; }
    public string ProjectTitle { get; set; } = string.Empty;
    public string CounterpartyId { get; set; } = string.Empty;
    public string CounterpartyName { get; set; } = string.Empty;
    public string? CounterpartyAvatarUrl { get; set; }
    public DateTime ProjectCompletedAt { get; set; }
    public ReviewerRole AsRole { get; set; }
}

public class CreateReviewRequest
{
    [Required]
    public Guid ProjectId { get; set; }

    [Required]
    public string FreelancerId { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Avaliação geral deve ser entre 1 e 5")]
    public int Rating { get; set; }

    [Range(1, 5)] public int? RatingQuality { get; set; }
    [Range(1, 5)] public int? RatingCommunication { get; set; }
    [Range(1, 5)] public int? RatingDeadline { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}

public class UpdateProfileRequest
{
    public const int BioMinLength = 80;
    public const int BioMaxLength = 2000;

    [Required(ErrorMessage = "Nome completo é obrigatório")]
    [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    [MaxLength(30, ErrorMessage = "Telefone deve ter no máximo 30 caracteres")]
    public string? PhoneNumber { get; set; }

    [MaxLength(120, ErrorMessage = "Cidade deve ter no máximo 120 caracteres")]
    public string? City { get; set; }

    [MaxLength(200, ErrorMessage = "Título profissional deve ter no máximo 200 caracteres")]
    public string? Title { get; set; }

    [StringLength(BioMaxLength, MinimumLength = BioMinLength,
        ErrorMessage = "Bio deve ter entre {2} e {1} caracteres")]
    public string? Bio { get; set; }

    public decimal? HourlyRate { get; set; }

    [MaxLength(500, ErrorMessage = "URL do portfólio deve ter no máximo 500 caracteres")]
    public string? PortfolioUrl { get; set; }

    public List<string> Skills { get; set; } = [];
}

public class CreateFreelancerReviewRequest
{
    [Required]
    public Guid ProjectId { get; set; }

    [Range(1, 5, ErrorMessage = "Avaliação geral deve ser entre 1 e 5")]
    public int Rating { get; set; }

    [Range(1, 5)] public int? RatingQuality { get; set; }
    [Range(1, 5)] public int? RatingCommunication { get; set; }
    [Range(1, 5)] public int? RatingDeadline { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}

public class ErrorResponse
{
    public string Message { get; set; } = string.Empty;
}

public class PlatformFeeDto
{
    public Guid Id { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public decimal ClientFee { get; set; }
    public decimal FreelancerFee { get; set; }
    public decimal CommissionPct { get; set; }
}

public class PublicStatsDto
{
    public int TotalProjects { get; set; }
    public int ActiveFreelancers { get; set; }
    public int CompletedProjects { get; set; }
    public int Categories { get; set; }
}

public class ClientStatsDto
{
    public int ActiveProjects { get; set; }
    public int ProposalsToReview { get; set; }
    public int Conversations { get; set; }
    public decimal TotalSpentApprox { get; set; }
}

public class FreelancerStatsDto
{
    public decimal EarningsThisMonth { get; set; }
    public double AcceptanceRate { get; set; }
    public int PendingProposals { get; set; }
    public int ActiveContracts { get; set; }
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
}

public class NotificationPreferencesDto
{
    public bool EmailProposalReceived { get; set; } = true;
    public bool EmailProposalAccepted { get; set; } = true;
    public bool EmailProposalRejected { get; set; } = true;
    public bool EmailProjectCompleted { get; set; } = true;
    public bool EmailChatOfflineDigest { get; set; } = true;
}
