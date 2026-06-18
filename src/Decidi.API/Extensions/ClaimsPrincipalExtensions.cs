using System.Security.Claims;

namespace Decidi.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Usuário não autenticado.");
    }

    public static string GetUserRole(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue("role") ?? string.Empty;
    }
}
