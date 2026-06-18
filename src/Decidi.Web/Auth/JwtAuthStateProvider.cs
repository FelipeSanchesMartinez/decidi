using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Decidi.Web.Auth;

public class JwtAuthStateProvider(IJSRuntime jsRuntime) : AuthenticationStateProvider
{
    private const string TokenKey = "authToken";
    private const string LegacyRefreshTokenKey = "refreshToken";

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenAsync();

        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var claims = ParseClaimsFromJwt(token);
        if (claims is null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await SetTokenAsync(token);
        // Refresh token vive em cookie HttpOnly gerenciado pelo servidor — nada para armazenar aqui.
        await ClearLegacyRefreshTokenAsync();

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var user = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await RemoveTokenAsync();
        await ClearLegacyRefreshTokenAsync();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(anonymous)));
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await jsRuntime.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        }
        catch
        {
            return null;
        }
    }

    public bool IsTokenExpiringSoon(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            return jwt.ValidTo < DateTime.UtcNow.AddMinutes(2);
        }
        catch
        {
            return true;
        }
    }

    private async Task SetTokenAsync(string token)
    {
        await jsRuntime.InvokeVoidAsync("localStorage.setItem", TokenKey, token);
    }

    private async Task RemoveTokenAsync()
    {
        await jsRuntime.InvokeVoidAsync("localStorage.removeItem", TokenKey);
    }

    private async Task ClearLegacyRefreshTokenAsync()
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("localStorage.removeItem", LegacyRefreshTokenKey);
        }
        catch { }
    }

    private static IEnumerable<Claim>? ParseClaimsFromJwt(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            if (jwt.ValidTo < DateTime.UtcNow)
                return null;

            var claims = jwt.Claims.ToList();
            var roleClaim = claims.FirstOrDefault(c => c.Type == "role");
            if (roleClaim is not null)
                claims.Add(new Claim(ClaimTypes.Role, roleClaim.Value));

            return claims;
        }
        catch
        {
            return null;
        }
    }
}
