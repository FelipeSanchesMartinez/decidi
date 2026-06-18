using System.ComponentModel.DataAnnotations;
using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Auth;

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

    [Range(typeof(bool), "true", "true", ErrorMessage = "Você precisa aceitar os Termos de Uso e a Política de Privacidade.")]
    public bool AcceptedTerms { get; set; }
}
