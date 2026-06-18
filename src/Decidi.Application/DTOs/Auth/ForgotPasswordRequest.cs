using System.ComponentModel.DataAnnotations;

namespace Decidi.Application.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required(ErrorMessage = "E-mail é obrigatório")]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string Email { get; set; } = string.Empty;
}
