using Decidi.Domain.Enums;

namespace Decidi.Application.DTOs.Auth;

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
