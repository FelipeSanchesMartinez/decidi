using Decidi.Application.DTOs.Auth;
using Decidi.Application.DTOs.Common;
using Decidi.Application.DTOs.Reviews;
using Decidi.Application.Interfaces;
using Decidi.Application.Mappings;
using Decidi.Domain.Entities;
using Decidi.Domain.Enums;
using Decidi.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Decidi.Application.Services;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    IEmailService emailService,
    ISanitizer sanitizer,
    IUnitOfWork unitOfWork,
    ISkillRepository skillRepository,
    IReviewRepository reviewRepository,
    IProjectRepository projectRepository) : IAuthService
{
    public const string CurrentTermsVersion = "2026-06-17";

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, string baseUrl)
    {
        if (!request.AcceptedTerms)
            throw new InvalidOperationException("Você precisa aceitar os Termos de Uso e a Política de Privacidade.");

        var existingUser = await userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
            throw new InvalidOperationException("E-mail já cadastrado.");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = sanitizer.Sanitize(request.FullName),
            Role = request.Role,
            TermsAcceptedAt = DateTime.UtcNow,
            TermsVersion = CurrentTermsVersion
        };

        if (request.Role == UserRole.Freelancer)
        {
            user.FreelancerProfile = new FreelancerProfile
            {
                Title = "Novo Freelancer",
                Bio = string.Empty
            };
        }

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        var emailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(emailToken);
        var confirmationLink = $"{baseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

        try
        {
            await emailService.SendEmailConfirmationAsync(user.Email!, user.FullName, confirmationLink);
        }
        catch
        {
            // best-effort: não quebra o cadastro se o e-mail falhar
        }

        // Auto-login imediato: princípio "cadastro em <30s".
        // RequiresEmailConfirmation=true sinaliza que ações sensíveis ainda precisam de confirmação.
        var (token, expiresAt) = tokenService.GenerateToken(user);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Role = user.Role,
            ExpiresAt = expiresAt,
            RequiresEmailConfirmation = true
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("Credenciais inválidas.");

        var validPassword = await userManager.CheckPasswordAsync(user, request.Password);
        if (!validPassword)
            throw new InvalidOperationException("Credenciais inválidas.");

        // Confirmação de e-mail NÃO bloqueia login (princípio do produto: <30s para começar a usar).
        // Ações sensíveis (publicar projeto, propor) podem checar claim email_confirmed.
        var emailConfirmed = await userManager.IsEmailConfirmedAsync(user);
        var (token, expiresAt) = tokenService.GenerateToken(user);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user);

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Role = user.Role,
            ExpiresAt = expiresAt,
            RequiresEmailConfirmation = !emailConfirmed
        };
    }

    public async Task<AuthResponse> ConfirmEmailAsync(string userId, string token)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            throw new InvalidOperationException("Token de confirmação inválido ou expirado.");

        // Auto-login pós-confirmação — usuário não precisa digitar credenciais de novo.
        var (jwt, expiresAt) = tokenService.GenerateToken(user);
        var refreshToken = await tokenService.GenerateRefreshTokenAsync(user);

        return new AuthResponse
        {
            Token = jwt,
            RefreshToken = refreshToken.Token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Role = user.Role,
            ExpiresAt = expiresAt,
            RequiresEmailConfirmation = false
        };
    }

    public async Task ResendConfirmationEmailAsync(string email, string baseUrl)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || await userManager.IsEmailConfirmedAsync(user)) return;

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var confirmationLink = $"{baseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";

        try
        {
            await emailService.SendEmailConfirmationAsync(user.Email!, user.FullName, confirmationLink);
        }
        catch
        {
            // best-effort
        }
    }

    public async Task ForgotPasswordAsync(string email, string baseUrl)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null) return;

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var resetLink = $"{baseUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={encodedToken}";

        try
        {
            await emailService.SendPasswordResetAsync(user.Email!, user.FullName, resetLink);
        }
        catch
        {
            // best-effort
        }
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        var (newToken, expiresAt, newRefreshToken, user) = await tokenService.RefreshAsync(request.Token, request.RefreshToken);

        return new AuthResponse
        {
            Token = newToken,
            RefreshToken = newRefreshToken.Token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            Role = user.Role,
            ExpiresAt = expiresAt
        };
    }

    public async Task<UserProfileDto> GetProfileAsync(string userId)
    {
        var user = await userManager.Users
            .Include(u => u.FreelancerProfile)
                .ThenInclude(fp => fp!.Skills)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        return user.ToProfileDto();
    }

    public async Task<UserProfileDto> UpdateProfileAsync(string userId, UpdateProfileRequest request)
    {
        var user = await userManager.Users
            .Include(u => u.FreelancerProfile)
                .ThenInclude(fp => fp!.Skills)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        user.FullName = sanitizer.Sanitize(request.FullName);
        user.AvatarUrl = request.AvatarUrl;
        user.City = sanitizer.Sanitize(request.City ?? string.Empty) is { Length: > 0 } city ? city : null;
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        if (user.Role == UserRole.Freelancer)
        {
            if (user.FreelancerProfile is null)
            {
                user.FreelancerProfile = new FreelancerProfile
                {
                    UserId = user.Id,
                    Title = "Novo Freelancer",
                    Bio = string.Empty
                };
            }

            user.FreelancerProfile.Title = sanitizer.Sanitize(request.Title) is { Length: > 0 } title
                ? title : user.FreelancerProfile.Title;
            user.FreelancerProfile.Bio = sanitizer.Sanitize(request.Bio) is { Length: > 0 } bio
                ? bio : user.FreelancerProfile.Bio;
            user.FreelancerProfile.HourlyRate = request.HourlyRate ?? user.FreelancerProfile.HourlyRate;
            user.FreelancerProfile.PortfolioUrl = request.PortfolioUrl;

            user.FreelancerProfile.Skills.Clear();
            if (request.Skills.Count > 0)
            {
                var existingSkills = (await skillRepository.GetByNamesAsync(request.Skills)).ToList();
                var existingNames = existingSkills.Select(s => s.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var skill in existingSkills)
                    user.FreelancerProfile.Skills.Add(skill);

                foreach (var name in request.Skills.Where(n => !existingNames.Contains(n)))
                {
                    var newSkill = new Skill { Name = name, Group = "Outros" };
                    await skillRepository.AddAsync(newSkill);
                    user.FreelancerProfile.Skills.Add(newSkill);
                }
            }
        }

        await unitOfWork.SaveChangesAsync();

        return user.ToProfileDto();
    }

    public async Task<string> SaveAvatarAsync(string userId, byte[] content, string extension)
    {
        var user = await userManager.FindByIdAsync(userId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = extension.ToLowerInvariant();
        if (!allowed.Contains(ext))
            throw new InvalidOperationException("Formato não suportado. Use JPG, PNG ou WebP.");

        if (content.Length == 0 || content.Length > 2 * 1024 * 1024)
            throw new InvalidOperationException("Arquivo inválido. Tamanho máximo: 2 MB.");

        var uploadsRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(uploadsRoot);

        // Limpa arquivos antigos do mesmo userId (qualquer extensão).
        foreach (var old in Directory.EnumerateFiles(uploadsRoot, $"{userId}.*"))
        {
            try { File.Delete(old); } catch { }
        }

        var fileName = $"{userId}{ext}";
        await File.WriteAllBytesAsync(Path.Combine(uploadsRoot, fileName), content);

        // Cache-busting com timestamp ticks (que o front pode comparar com o user.UpdatedAt).
        user.AvatarUrl = $"/uploads/avatars/{fileName}?v={DateTime.UtcNow.Ticks}";
        await userManager.UpdateAsync(user);

        return user.AvatarUrl;
    }

    public async Task<PublicProfileDto> GetPublicProfileAsync(string userId)
    {
        var user = await userManager.Users
            .Include(u => u.FreelancerProfile)
                .ThenInclude(fp => fp!.Skills)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        // Perfil público de freelancer mostra reviews recebidas DE clientes (quem avaliou = Client).
        var reviews = await reviewRepository.GetByFreelancerIdAsync(userId, onlyReleased: true);
        var avgRating = await reviewRepository.GetAverageRatingAsync(userId, ReviewerRole.Client);
        var totalReviews = await reviewRepository.GetCountAsync(userId, ReviewerRole.Client);

        var allProjects = await projectRepository.GetProjectsWithDetailsAsync(status: ProjectStatus.Completed);
        var completedAsFreelancer = allProjects.Count(p => p.AcceptedFreelancerId == userId);

        return new PublicProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            Title = user.FreelancerProfile?.Title,
            Bio = user.FreelancerProfile?.Bio,
            HourlyRate = user.FreelancerProfile?.HourlyRate,
            PortfolioUrl = user.FreelancerProfile?.PortfolioUrl,
            Skills = user.FreelancerProfile?.Skills.Select(s => s.ToDto()).ToList() ?? [],
            Reviews = reviews.Select(r => r.ToDto()).ToList(),
            AverageRating = avgRating,
            TotalReviews = totalReviews,
            CompletedProjects = completedAsFreelancer
        };
    }
}
