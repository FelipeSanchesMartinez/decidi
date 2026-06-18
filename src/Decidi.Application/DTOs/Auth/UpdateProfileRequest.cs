using System.ComponentModel.DataAnnotations;

namespace Decidi.Application.DTOs.Auth;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Nome completo é obrigatório")]
    [MaxLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
    public string FullName { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    [MaxLength(30, ErrorMessage = "Telefone deve ter no máximo 30 caracteres")]
    [Phone(ErrorMessage = "Telefone inválido")]
    public string? PhoneNumber { get; set; }

    [MaxLength(120, ErrorMessage = "Cidade deve ter no máximo 120 caracteres")]
    public string? City { get; set; }

    // Freelancer-specific.
    [MaxLength(200, ErrorMessage = "Título profissional deve ter no máximo 200 caracteres")]
    public string? Title { get; set; }

    // Bio é opcional, mas se preenchida deve ter entre 80 e 2000 caracteres.
    // Mínimo de 80 garante apresentação minimamente útil para o cliente; máximo respeita o limite do banco.
    [StringLength(2000, MinimumLength = 80, ErrorMessage = "Bio deve ter entre {2} e {1} caracteres")]
    public string? Bio { get; set; }

    public decimal? HourlyRate { get; set; }

    [MaxLength(500, ErrorMessage = "URL do portfólio deve ter no máximo 500 caracteres")]
    public string? PortfolioUrl { get; set; }

    public List<string> Skills { get; set; } = [];
}
