using System.Security.Claims;
using Decidi.Web.Auth;
using Decidi.Web.Models;

namespace Decidi.Web.Services;

public class AppState
{
    private readonly ApiClient _api;
    private readonly JwtAuthStateProvider _authState;

    // Caches simples por sessão: catálogos pouco voláteis.
    private Task<IReadOnlyList<CategoryDto>>? _categoriesCache;
    private Task<IReadOnlyList<SkillDto>>? _skillsCache;

    public AppState(ApiClient api, JwtAuthStateProvider authState)
    {
        _api = api;
        _authState = authState;
    }

    public bool IsAuthenticated { get; private set; }
    public bool IsFreelancer { get; private set; }
    public bool IsClient { get; private set; }
    public bool IsProfileComplete { get; private set; }
    public string UserName { get; private set; } = "";
    public string UserId { get; private set; } = "";
    public UserProfileDto? Profile { get; private set; }
    public int UnreadNotificationCount { get; private set; }

    public event Action? OnChange;

    public async Task InitializeAsync()
    {
        var auth = await _authState.GetAuthenticationStateAsync();
        IsAuthenticated = auth.User.Identity?.IsAuthenticated == true;

        if (!IsAuthenticated)
        {
            Reset();
            NotifyChanged();
            return;
        }

        UserName = auth.User.FindFirst(ClaimTypes.Name)?.Value ?? "Usuário";
        UserId = auth.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
        var role = auth.User.FindFirst("role")?.Value;
        IsFreelancer = role == "Freelancer";
        IsClient = role == "Client";

        try
        {
            Profile = await _api.GetProfileAsync();
            if (Profile is not null && IsFreelancer)
            {
                IsProfileComplete = !string.IsNullOrWhiteSpace(Profile.Title)
                    && Profile.Title != "Novo Freelancer"
                    && !string.IsNullOrWhiteSpace(Profile.Bio)
                    && Profile.Skills.Count > 0;
            }
            else
            {
                IsProfileComplete = true;
            }
        }
        catch
        {
            IsProfileComplete = false;
        }

        try
        {
            UnreadNotificationCount = await _api.GetUnreadNotificationCountAsync();
        }
        catch
        {
            UnreadNotificationCount = 0;
        }

        NotifyChanged();
    }

    public void SetUnreadCount(int count)
    {
        UnreadNotificationCount = count;
        NotifyChanged();
    }

    public void DecrementUnreadCount()
    {
        UnreadNotificationCount = Math.Max(0, UnreadNotificationCount - 1);
        NotifyChanged();
    }

    public void IncrementUnreadCount()
    {
        UnreadNotificationCount += 1;
        NotifyChanged();
    }

    public void MarkProfileComplete()
    {
        IsProfileComplete = true;
        NotifyChanged();
    }

    public List<string> GetMissingFields()
    {
        var missing = new List<string>();
        if (Profile is null) return ["perfil (erro ao carregar)"];
        if (string.IsNullOrWhiteSpace(Profile.Title) || Profile.Title == "Novo Freelancer")
            missing.Add("título profissional");
        if (string.IsNullOrWhiteSpace(Profile.Bio))
            missing.Add("bio (sobre mim)");
        if (Profile.Skills.Count == 0)
            missing.Add("habilidades");
        return missing;
    }

    /// <summary>
    /// Lista de categorias da plataforma. Cache em memória por sessão.
    /// Em caso de falha, retorna lista vazia e descarta o cache para tentar de novo na próxima chamada.
    /// </summary>
    public Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync()
        => _categoriesCache ??= LoadCategoriesAsync();

    /// <summary>
    /// Lista de skills da plataforma. Cache em memória por sessão.
    /// </summary>
    public Task<IReadOnlyList<SkillDto>> GetSkillsAsync()
        => _skillsCache ??= LoadSkillsAsync();

    public void InvalidateCatalogs()
    {
        _categoriesCache = null;
        _skillsCache = null;
    }

    private async Task<IReadOnlyList<CategoryDto>> LoadCategoriesAsync()
    {
        try
        {
            var result = await _api.GetCategoriesAsync();
            return result?.ToList() ?? [];
        }
        catch
        {
            _categoriesCache = null; // permite retry
            return [];
        }
    }

    private async Task<IReadOnlyList<SkillDto>> LoadSkillsAsync()
    {
        try
        {
            var result = await _api.GetSkillsAsync();
            return result?.ToList() ?? [];
        }
        catch
        {
            _skillsCache = null; // permite retry
            return [];
        }
    }

    private void Reset()
    {
        UserName = "";
        UserId = "";
        IsFreelancer = false;
        IsClient = false;
        IsProfileComplete = false;
        Profile = null;
        UnreadNotificationCount = 0;
        // Catálogos não dependem de usuário; mantém o cache para acelerar a próxima sessão.
    }

    private void NotifyChanged() => OnChange?.Invoke();
}
