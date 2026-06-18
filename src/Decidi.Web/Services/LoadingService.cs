namespace Decidi.Web.Services;

public class LoadingService
{
    private int _activeRequests;

    public bool IsLoading => _activeRequests > 0;

    public event Action? OnChange;

    public void Show()
    {
        Interlocked.Increment(ref _activeRequests);
        OnChange?.Invoke();
    }

    public void Hide()
    {
        if (Interlocked.Decrement(ref _activeRequests) < 0)
            Interlocked.Exchange(ref _activeRequests, 0);
        OnChange?.Invoke();
    }
}
