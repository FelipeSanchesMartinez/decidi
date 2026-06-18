using Decidi.API.Extensions;
using Decidi.Application.DTOs.Auth;
using Decidi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Decidi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    ITokenService tokenService,
    IConfiguration configuration) : ControllerBase
{
    private const string RefreshCookie = "decidi.refresh";

    private string WebBaseUrl => configuration["WebBaseUrl"] ?? "https://localhost:5002";

    private void SetRefreshCookie(string refreshToken)
    {
        Response.Cookies.Append(RefreshCookie, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            IsEssential = true
        });
    }

    private void ClearRefreshCookie()
    {
        Response.Cookies.Delete(RefreshCookie, new CookieOptions
        {
            Path = "/api/auth",
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await authService.RegisterAsync(request, WebBaseUrl);
            if (!string.IsNullOrEmpty(response.RefreshToken))
            {
                SetRefreshCookie(response.RefreshToken);
                response.RefreshToken = string.Empty;
            }
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var response = await authService.LoginAsync(request);
            if (!string.IsNullOrEmpty(response.RefreshToken))
            {
                SetRefreshCookie(response.RefreshToken);
                // Não devolve refresh token no body: vai apenas no cookie HttpOnly.
                response.RefreshToken = string.Empty;
            }
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        // Sem [Authorize]: precisa funcionar mesmo com access token expirado.
        var refreshToken = Request.Cookies[RefreshCookie];
        if (!string.IsNullOrEmpty(refreshToken))
            await tokenService.RevokeRefreshTokenAsync(refreshToken);
        ClearRefreshCookie();
        return NoContent();
    }

    [HttpPost("confirm-email")]
    public async Task<ActionResult<AuthResponse>> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        try
        {
            var response = await authService.ConfirmEmailAsync(request.UserId, request.Token);
            if (!string.IsNullOrEmpty(response.RefreshToken))
            {
                SetRefreshCookie(response.RefreshToken);
                response.RefreshToken = string.Empty;
            }
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("resend-confirmation")]
    public async Task<ActionResult<MessageResponse>> ResendConfirmation([FromBody] ForgotPasswordRequest request)
    {
        await authService.ResendConfirmationEmailAsync(request.Email, WebBaseUrl);
        return Ok(new MessageResponse { Message = "Se o e-mail estiver cadastrado e pendente de confirmação, um novo link será enviado." });
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<MessageResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        await authService.ForgotPasswordAsync(request.Email, WebBaseUrl);
        return Ok(new MessageResponse { Message = "Se o e-mail estiver cadastrado, você receberá instruções para redefinir sua senha." });
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<MessageResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            await authService.ResetPasswordAsync(request);
            return Ok(new MessageResponse { Message = "Senha redefinida com sucesso!" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<AuthResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Prefere o cookie HttpOnly; usa o body como fallback durante a transição.
            var refreshFromCookie = Request.Cookies[RefreshCookie];
            if (!string.IsNullOrEmpty(refreshFromCookie))
                request.RefreshToken = refreshFromCookie;

            if (string.IsNullOrEmpty(request.RefreshToken))
                return Unauthorized(new { message = "Token inválido ou expirado." });

            var response = await authService.RefreshTokenAsync(request);

            if (!string.IsNullOrEmpty(response.RefreshToken))
            {
                SetRefreshCookie(response.RefreshToken);
                response.RefreshToken = string.Empty;
            }

            return Ok(response);
        }
        catch (Exception)
        {
            ClearRefreshCookie();
            return Unauthorized(new { message = "Token inválido ou expirado." });
        }
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        var userId = User.GetUserId();
        var profile = await authService.GetProfileAsync(userId);
        return Ok(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = User.GetUserId();
            var profile = await authService.UpdateProfileAsync(userId, request);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("avatar")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<ActionResult<object>> UploadAvatar(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "Nenhum arquivo enviado." });

        try
        {
            var userId = User.GetUserId();
            var ext = Path.GetExtension(file.FileName);
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var url = await authService.SaveAvatarAsync(userId, ms.ToArray(), ext);
            return Ok(new { avatarUrl = url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("profile/{userId}")]
    public async Task<ActionResult<PublicProfileDto>> GetPublicProfile(string userId)
    {
        try
        {
            var profile = await authService.GetPublicProfileAsync(userId);
            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
