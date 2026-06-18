using Decidi.Domain.Entities;

namespace Decidi.Application.Interfaces;

public interface ITokenService
{
    (string token, DateTime expiresAt) GenerateToken(ApplicationUser user);
    Task<RefreshToken> GenerateRefreshTokenAsync(ApplicationUser user);
    Task<(string token, DateTime expiresAt, RefreshToken refreshToken, ApplicationUser user)> RefreshAsync(string token, string refreshToken);
    Task RevokeRefreshTokenAsync(string refreshToken);
}
