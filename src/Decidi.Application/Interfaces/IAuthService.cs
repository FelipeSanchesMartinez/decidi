using Decidi.Application.DTOs.Auth;

namespace Decidi.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, string baseUrl);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> ConfirmEmailAsync(string userId, string token);
    Task ResendConfirmationEmailAsync(string email, string baseUrl);
    Task ForgotPasswordAsync(string email, string baseUrl);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task<UserProfileDto> GetProfileAsync(string userId);
    Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    Task<PublicProfileDto> GetPublicProfileAsync(string userId);
    Task<string> SaveAvatarAsync(string userId, byte[] content, string extension);
}
