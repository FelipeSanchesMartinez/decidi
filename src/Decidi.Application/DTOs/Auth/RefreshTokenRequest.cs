using System.ComponentModel.DataAnnotations;

namespace Decidi.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;

    // Opcional: se ausente no body, o controller usa o cookie HttpOnly.
    public string RefreshToken { get; set; } = string.Empty;
}
