namespace Decidi.Web.Services;

public enum ToastLevel { Success, Warning, Error, Info }

public record ToastMessage(string Title, string Message, ToastLevel Level, DateTime CreatedAt);

public class ToastService
{
    private const int AutoDismissMs = 5000;
    private readonly List<ToastMessage> _toasts = [];
    private readonly Lock _lock = new();

    public IReadOnlyList<ToastMessage> Toasts
    {
        get { lock (_lock) return _toasts.ToList(); }
    }

    public event Action? OnChange;

    public void Show(string title, string message, ToastLevel level = ToastLevel.Info)
    {
        var toast = new ToastMessage(title, message, level, DateTime.Now);
        lock (_lock) _toasts.Add(toast);
        OnChange?.Invoke();
        _ = RemoveAfterDelay(toast);
    }

    public void ShowSuccess(string message) => Show("Sucesso", message, ToastLevel.Success);
    public void ShowError(string message) => Show("Erro", message, ToastLevel.Error);
    public void ShowWarning(string message) => Show("Atenção", message, ToastLevel.Warning);
    public void ShowInfo(string message) => Show("Informação", message, ToastLevel.Info);

    public void Remove(ToastMessage toast)
    {
        bool removed;
        lock (_lock) removed = _toasts.Remove(toast);
        if (removed) OnChange?.Invoke();
    }

    private async Task RemoveAfterDelay(ToastMessage toast)
    {
        await Task.Delay(AutoDismissMs);
        Remove(toast);
    }
}
