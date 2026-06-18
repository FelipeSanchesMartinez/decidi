using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Decidi.Web.Models;
using Decidi.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace Decidi.Web.Auth;

public class GlobalHttpHandler(
    JwtAuthStateProvider authStateProvider,
    ToastService toastService,
    LoadingService loadingService,
    IHttpClientFactory httpClientFactory,
    NavigationManager navigation) : DelegatingHandler
{
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);
    private static Task<string?>? _inflightRefresh;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Endpoints de auth que emitem/leem o cookie HttpOnly precisam viajar com credenciais.
        if (NeedsCredentials(request))
            request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

        var token = await authStateProvider.GetTokenAsync();
        var isRefresh = IsRefreshRequest(request);

        if (!string.IsNullOrEmpty(token) && !isRefresh && authStateProvider.IsTokenExpiringSoon(token))
            token = await GetOrStartRefreshAsync(token) ?? token;

        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await SendWithFeedbackAsync(request, cancellationToken);
        if (response is null) return BuildFailure(HttpStatusCode.ServiceUnavailable);

        // Retry-on-401: tenta renovar uma vez antes de deslogar.
        // Refresh request nunca retenta. Multipart também não — streams de upload (avatar)
        // costumam ser não-seekáveis e ReadAsByteArrayAsync após a primeira tentativa vira lixo.
        if (response.StatusCode == HttpStatusCode.Unauthorized
            && !isRefresh
            && !string.IsNullOrEmpty(token)
            && CanCloneSafely(request))
        {
            response.Dispose();
            var newToken = await GetOrStartRefreshAsync(token);
            if (!string.IsNullOrEmpty(newToken))
            {
                var retry = await CloneRequestAsync(request);
                retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                var retried = await SendWithFeedbackAsync(retry, cancellationToken);
                if (retried is null) return BuildFailure(HttpStatusCode.ServiceUnavailable);
                if (retried.IsSuccessStatusCode) return retried;
                response = retried;
            }
            else
            {
                response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }

        if (response.IsSuccessStatusCode)
            return response;

        var errorMessage = await ExtractErrorMessage(response);

        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                await authStateProvider.MarkUserAsLoggedOut();
                toastService.ShowWarning("Sua sessão expirou. Faça login novamente.");
                navigation.NavigateTo("/login");
                break;

            case HttpStatusCode.Forbidden:
                toastService.ShowError(errorMessage ?? "Você não tem permissão para realizar esta ação.");
                break;

            case HttpStatusCode.NotFound:
                toastService.ShowError(errorMessage ?? "O recurso solicitado não foi encontrado.");
                break;

            case HttpStatusCode.BadRequest:
                toastService.ShowError(errorMessage ?? "Dados inválidos. Verifique as informações e tente novamente.");
                break;

            case HttpStatusCode.Conflict:
                toastService.ShowError(errorMessage ?? "Conflito ao processar a requisição.");
                break;

            case HttpStatusCode.InternalServerError:
                toastService.ShowError("Ocorreu um erro interno no servidor. Tente novamente mais tarde.");
                break;

            default:
                toastService.ShowError(errorMessage ?? "Ocorreu um erro inesperado. Tente novamente.");
                break;
        }

        return response;
    }

    private async Task<HttpResponseMessage?> SendWithFeedbackAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        loadingService.Show();
        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            toastService.ShowError("Não foi possível conectar ao servidor. Verifique sua conexão.");
            return null;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            toastService.ShowError("A requisição expirou. Tente novamente.");
            return null;
        }
        finally
        {
            loadingService.Hide();
        }
    }

    private static HttpResponseMessage BuildFailure(HttpStatusCode code) => new(code);

    private async Task<string?> GetOrStartRefreshAsync(string currentToken)
    {
        Task<string?> task;
        await RefreshLock.WaitAsync();
        try
        {
            if (_inflightRefresh is null || _inflightRefresh.IsCompleted)
                _inflightRefresh = DoRefreshAsync(currentToken);
            task = _inflightRefresh;
        }
        finally
        {
            RefreshLock.Release();
        }
        return await task;
    }

    private async Task<string?> DoRefreshAsync(string currentToken)
    {
        try
        {
            var client = httpClientFactory.CreateClient("Decidi.API.NoAuth");
            var req = new HttpRequestMessage(HttpMethod.Post, "api/auth/refresh-token")
            {
                Content = JsonContent.Create(new { token = currentToken, refreshToken = string.Empty })
            };
            req.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);

            var response = await client.SendAsync(req);
            if (!response.IsSuccessStatusCode) return null;

            var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (auth is null || string.IsNullOrEmpty(auth.Token)) return null;

            await authStateProvider.MarkUserAsAuthenticated(auth.Token);
            return auth.Token;
        }
        catch
        {
            return null;
        }
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage src)
    {
        var clone = new HttpRequestMessage(src.Method, src.RequestUri)
        {
            Version = src.Version,
            VersionPolicy = src.VersionPolicy
        };

        foreach (var header in src.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        foreach (var option in src.Options)
            clone.Options.TryAdd(option.Key, option.Value);

        if (src.Content is not null)
        {
            var bytes = await src.Content.ReadAsByteArrayAsync();
            var content = new ByteArrayContent(bytes);
            foreach (var header in src.Content.Headers)
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            clone.Content = content;
        }

        return clone;
    }

    private static bool IsRefreshRequest(HttpRequestMessage request)
        => request.RequestUri?.PathAndQuery.Contains("refresh-token", StringComparison.OrdinalIgnoreCase) ?? false;

    /// <summary>
    /// Multipart/form-data tipicamente carrega streams não-seekáveis (InputFile);
    /// reler o body após o primeiro SendAsync devolveria conteúdo vazio.
    /// Para esses casos, deixa o 401 cair direto no logout — usuário refaz o upload.
    /// </summary>
    private static bool CanCloneSafely(HttpRequestMessage request)
        => request.Content is not MultipartContent;

    private static bool NeedsCredentials(HttpRequestMessage request)
    {
        var path = request.RequestUri?.PathAndQuery ?? string.Empty;
        return path.Contains("/api/auth/login", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/auth/refresh-token", StringComparison.OrdinalIgnoreCase)
            || path.Contains("/api/auth/logout", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<string?> ExtractErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content)) return null;

            var error = System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(
                content,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return error?.Message;
        }
        catch
        {
            return null;
        }
    }
}
