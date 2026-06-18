using System.Net.Http.Json;
using Decidi.Web.Models;

namespace Decidi.Web.Services;

public class ApiClient(HttpClient http)
{
    // Auth
    public Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        => http.PostAsJsonAsync("api/auth/register", request).ParseAsync<AuthResponse>();

    public Task<AuthResponse?> LoginAsync(LoginRequest request)
        => http.PostAsJsonAsync("api/auth/login", request).ParseAsync<AuthResponse>();

    public Task<AuthResponse?> ConfirmEmailAsync(string userId, string token)
        => http.PostAsJsonAsync("api/auth/confirm-email", new { userId, token }).ParseAsync<AuthResponse>();

    public async Task ResendConfirmationAsync(string email)
        => await http.PostAsJsonAsync("api/auth/resend-confirmation", new { email });

    public async Task ForgotPasswordAsync(string email)
        => await http.PostAsJsonAsync("api/auth/forgot-password", new { email });

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, string confirmPassword)
    {
        var response = await http.PostAsJsonAsync("api/auth/reset-password", new { email, token, newPassword, confirmPassword });
        return response.IsSuccessStatusCode;
    }

    public Task<AuthResponse?> RefreshTokenAsync(string token)
        => http.PostAsJsonAsync("api/auth/refresh-token", new { token, refreshToken = string.Empty })
            .ParseAsync<AuthResponse>();

    public async Task LogoutAsync()
    {
        try { await http.PostAsync("api/auth/logout", null); } catch { }
    }

    public Task<UserProfileDto?> GetProfileAsync()
        => http.GetFromJsonAsync<UserProfileDto>("api/auth/profile");

    public Task<UserProfileDto?> UpdateProfileAsync(UpdateProfileRequest request)
        => http.PutAsJsonAsync("api/auth/profile", request).ParseAsync<UserProfileDto>();

    public Task<PublicProfileDto?> GetPublicProfileAsync(string userId)
        => http.GetFromJsonAsync<PublicProfileDto>($"api/auth/profile/{userId}");

    public async Task<string?> UploadAvatarAsync(Stream content, string fileName, string contentType)
    {
        using var form = new MultipartFormDataContent();
        var stream = new StreamContent(content);
        stream.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(stream, "file", fileName);
        var response = await http.PostAsync("api/auth/avatar", form);
        if (!response.IsSuccessStatusCode) return null;
        var payload = await response.Content.ReadFromJsonAsync<AvatarUploadResponse>();
        return payload?.AvatarUrl;
    }

    private record AvatarUploadResponse(string AvatarUrl);

    // Projects
    public Task<PagedResult<ProjectListDto>?> SearchProjectsAsync(
        string? search = null, Guid? categoryId = null, ProjectStatus? status = null,
        decimal? budgetMin = null, decimal? budgetMax = null,
        ProjectBudgetType? budgetType = null, string? skill = null,
        string? sortBy = null, int page = 1, int pageSize = 10)
    {
        var query = $"api/projects?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search)) query += $"&search={Uri.EscapeDataString(search)}";
        if (categoryId.HasValue) query += $"&categoryId={categoryId}";
        if (status.HasValue) query += $"&status={(int)status}";
        if (budgetMin.HasValue) query += $"&budgetMin={budgetMin}";
        if (budgetMax.HasValue) query += $"&budgetMax={budgetMax}";
        if (budgetType.HasValue) query += $"&budgetType={(int)budgetType}";
        if (!string.IsNullOrWhiteSpace(skill)) query += $"&skill={Uri.EscapeDataString(skill)}";
        if (!string.IsNullOrWhiteSpace(sortBy)) query += $"&sortBy={Uri.EscapeDataString(sortBy)}";
        return http.GetFromJsonAsync<PagedResult<ProjectListDto>>(query);
    }

    public Task<ProjectDto?> GetProjectAsync(Guid id)
        => http.GetFromJsonAsync<ProjectDto>($"api/projects/{id}");

    public Task<ProjectDto?> CreateProjectAsync(CreateProjectRequest request)
        => http.PostAsJsonAsync("api/projects", request).ParseAsync<ProjectDto>();

    public Task<IEnumerable<ProjectListDto>?> GetMyProjectsAsync()
        => http.GetFromJsonAsync<IEnumerable<ProjectListDto>>("api/projects/my");

    public Task<ProjectDto?> StartProjectAsync(Guid id)
        => http.PostAsJsonAsync($"api/projects/{id}/start", new { }).ParseAsync<ProjectDto>();

    public Task<ProjectDto?> CompleteProjectAsync(Guid id)
        => http.PostAsJsonAsync($"api/projects/{id}/complete", new { }).ParseAsync<ProjectDto>();

    public Task<ProjectDto?> CancelProjectAsync(Guid id, string? reason)
        => http.PostAsJsonAsync($"api/projects/{id}/cancel", new { reason }).ParseAsync<ProjectDto>();

    // Proposals
    public Task<ProposalDto?> CreateProposalAsync(CreateProposalRequest request)
        => http.PostAsJsonAsync("api/proposals", request).ParseAsync<ProposalDto>();

    public Task<IEnumerable<ProposalDto>?> GetProjectProposalsAsync(Guid projectId)
        => http.GetFromJsonAsync<IEnumerable<ProposalDto>>($"api/proposals/project/{projectId}");

    public Task<IEnumerable<ProposalDto>?> GetMyProposalsAsync()
        => http.GetFromJsonAsync<IEnumerable<ProposalDto>>("api/proposals/my");

    public Task<ProposalDto?> AcceptProposalAsync(Guid id)
        => http.PutAsJsonAsync($"api/proposals/{id}/accept", new { }).ParseAsync<ProposalDto>();

    public Task<ProposalDto?> RejectProposalAsync(Guid id)
        => http.PutAsJsonAsync($"api/proposals/{id}/reject", new { }).ParseAsync<ProposalDto>();

    public Task WithdrawProposalAsync(Guid id)
        => http.PutAsJsonAsync($"api/proposals/{id}/withdraw", new { });

    // Chat
    public Task<IEnumerable<ConversationDto>?> GetConversationsAsync()
        => http.GetFromJsonAsync<IEnumerable<ConversationDto>>("api/chat/conversations");

    public Task<IEnumerable<MessageDto>?> GetMessagesAsync(Guid conversationId)
        => http.GetFromJsonAsync<IEnumerable<MessageDto>>($"api/chat/conversations/{conversationId}/messages");

    public Task<ConversationDto?> CreateConversationAsync(Guid projectId, string freelancerId)
        => http.PostAsJsonAsync($"api/chat/conversations/{projectId}/{freelancerId}", new { }).ParseAsync<ConversationDto>();

    public Task MarkAsReadAsync(Guid conversationId)
        => http.PutAsJsonAsync($"api/chat/conversations/{conversationId}/read", new { });

    public Task<MessageDto?> SendMessageAsync(SendMessageRequest request)
        => http.PostAsJsonAsync("api/chat/messages", request).ParseAsync<MessageDto>();

    // Milestones
    public Task<MilestoneDto?> CreateMilestoneAsync(CreateMilestoneRequest request)
        => http.PostAsJsonAsync("api/milestones", request).ParseAsync<MilestoneDto>();

    public Task<IEnumerable<MilestoneDto>?> GetProjectMilestonesAsync(Guid projectId)
        => http.GetFromJsonAsync<IEnumerable<MilestoneDto>>($"api/milestones/project/{projectId}");

    public Task<MilestoneDto?> UpdateMilestoneStatusAsync(Guid id, UpdateMilestoneStatusRequest request)
        => http.PutAsJsonAsync($"api/milestones/{id}/status", request).ParseAsync<MilestoneDto>();

    // Fees
    public Task<PlatformFeeDto?> GetCurrentFeesAsync()
        => http.GetFromJsonAsync<PlatformFeeDto>("api/fees/current");

    // Public stats
    public Task<PublicStatsDto?> GetPublicStatsAsync()
        => http.GetFromJsonAsync<PublicStatsDto>("api/stats/public");

    public Task<ClientStatsDto?> GetClientStatsAsync()
        => http.GetFromJsonAsync<ClientStatsDto>("api/stats/client");

    public Task<FreelancerStatsDto?> GetFreelancerStatsAsync()
        => http.GetFromJsonAsync<FreelancerStatsDto>("api/stats/freelancer");

    public Task<NotificationPreferencesDto?> GetNotificationPreferencesAsync()
        => http.GetFromJsonAsync<NotificationPreferencesDto>("api/notification-preferences");

    public Task<NotificationPreferencesDto?> UpdateNotificationPreferencesAsync(NotificationPreferencesDto prefs)
        => http.PutAsJsonAsync("api/notification-preferences", prefs).ParseAsync<NotificationPreferencesDto>();

    // Categories
    public Task<IEnumerable<CategoryDto>?> GetCategoriesAsync()
        => http.GetFromJsonAsync<IEnumerable<CategoryDto>>("api/categories");

    // Skills
    public Task<IEnumerable<SkillDto>?> GetSkillsAsync()
        => http.GetFromJsonAsync<IEnumerable<SkillDto>>("api/skills");

    // Notifications
    public Task<IEnumerable<NotificationDto>?> GetNotificationsAsync()
        => http.GetFromJsonAsync<IEnumerable<NotificationDto>>("api/notifications");

    public Task<int> GetUnreadNotificationCountAsync()
        => http.GetFromJsonAsync<int>("api/notifications/unread-count");

    public Task MarkNotificationAsReadAsync(Guid id)
        => http.PutAsJsonAsync($"api/notifications/{id}/read", new { });

    public Task MarkAllNotificationsAsReadAsync()
        => http.PutAsJsonAsync("api/notifications/read-all", new { });

    // Reviews
    public Task<ReviewDto?> CreateReviewAsync(CreateReviewRequest request)
        => http.PostAsJsonAsync("api/reviews", request).ParseAsync<ReviewDto>();

    public Task<IEnumerable<ReviewDto>?> GetFreelancerReviewsAsync(string freelancerId)
        => http.GetFromJsonAsync<IEnumerable<ReviewDto>>($"api/reviews/freelancer/{freelancerId}");

    public Task<ReviewDto?> CreateFreelancerReviewAsync(CreateFreelancerReviewRequest request)
        => http.PostAsJsonAsync("api/reviews/from-freelancer", request).ParseAsync<ReviewDto>();

    public Task<IEnumerable<ReviewDto>?> GetClientReviewsAsync(string clientId)
        => http.GetFromJsonAsync<IEnumerable<ReviewDto>>($"api/reviews/client/{clientId}");

    public Task<IEnumerable<PendingReviewDto>?> GetPendingReviewsAsync()
        => http.GetFromJsonAsync<IEnumerable<PendingReviewDto>>("api/reviews/pending");
}

public static class HttpResponseExtensions
{
    public static async Task<T?> ParseAsync<T>(this Task<HttpResponseMessage> responseTask)
    {
        var response = await responseTask;
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>();
        return default;
    }
}
