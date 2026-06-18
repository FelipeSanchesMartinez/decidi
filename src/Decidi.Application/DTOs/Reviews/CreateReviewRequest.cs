using System.ComponentModel.DataAnnotations;

namespace Decidi.Application.DTOs.Reviews;

public class CreateReviewRequest
{
    [Required(ErrorMessage = "Projeto é obrigatório")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Freelancer é obrigatório")]
    public string FreelancerId { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Avaliação geral deve ser entre 1 e 5")]
    public int Rating { get; set; }

    [Range(1, 5)] public int? RatingQuality { get; set; }
    [Range(1, 5)] public int? RatingCommunication { get; set; }
    [Range(1, 5)] public int? RatingDeadline { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }
}
