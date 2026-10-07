using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace JwtDecoder.App.Services;

/// <summary>
/// Reads and writes plain text on the device clipboard, always on the main thread as some platforms require
/// </summary>
public sealed class ClipboardService(IClipboard clipboard)
{
    public Task SetTextAsync(string text) =>
        MainThread.InvokeOnMainThreadAsync(() => clipboard.SetTextAsync(text));

    public Task<string?> GetTextAsync() =>
        MainThread.InvokeOnMainThreadAsync(() => clipboard.HasText ? clipboard.GetTextAsync() : Task.FromResult<string?>(null));
}
