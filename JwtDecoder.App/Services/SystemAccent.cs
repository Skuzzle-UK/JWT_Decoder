namespace JwtDecoder.App.Services;

/// <summary>
/// The user's system accent colour, in the shades each platform uses on light and dark backgrounds
/// </summary>
public static class SystemAccent
{
    /// <summary>
    /// CSS colours for the accent on a light and a dark background, or nulls to use the app's defaults
    /// </summary>
    public static (string? Light, string? Dark) GetCssColors()
    {
        try
        {
            return GetPlatformColors();
        }
        catch (Exception)
        {
            // Purely cosmetic, so fall back to the default accent if the platform can't provide one
            return (null, null);
        }
    }

    /// <summary>
    /// Inline style setting the accent CSS variables used by app.css
    /// </summary>
    public static string? GetCssVariables()
    {
        (string? light, string? dark) = GetCssColors();
        return light is null || dark is null
            ? null
            : $"--system-accent-light: {light}; --system-accent-dark: {dark};";
    }

    private static (string? Light, string? Dark) GetPlatformColors()
    {
#if WINDOWS
        // Windows itself uses the darker shade on light backgrounds and the lighter shade on dark ones
        Windows.UI.ViewManagement.UISettings settings = new();
        return (
            ToCss(settings.GetColorValue(Windows.UI.ViewManagement.UIColorType.AccentDark1)),
            ToCss(settings.GetColorValue(Windows.UI.ViewManagement.UIColorType.AccentLight2)));

        static string ToCss(Windows.UI.Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
#elif ANDROID
        // Material You dynamic colour (Android 12+)
        if (!OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            return (null, null);
        }

        Android.Content.Res.Resources resources = Android.App.Application.Context.Resources!;
        return (
            ToCss(resources.GetColor(Android.Resource.Color.SystemAccent1600, null)),
            ToCss(resources.GetColor(Android.Resource.Color.SystemAccent1200, null)));

        static string ToCss(int argb) => $"#{argb & 0xFFFFFF:X6}";
#elif IOS || MACCATALYST
        // The app tint follows the system accent colour on macOS; on iOS it's the system blue
        UIKit.UIColor tint = UIKit.UIColor.Tint;
        return (
            ToCss(tint.GetResolvedColor(UIKit.UITraitCollection.FromUserInterfaceStyle(UIKit.UIUserInterfaceStyle.Light))),
            ToCss(tint.GetResolvedColor(UIKit.UITraitCollection.FromUserInterfaceStyle(UIKit.UIUserInterfaceStyle.Dark))));

        static string ToCss(UIKit.UIColor color)
        {
            color.GetRGBA(out nfloat r, out nfloat g, out nfloat b, out _);
            return $"#{(int)(r * 255):X2}{(int)(g * 255):X2}{(int)(b * 255):X2}";
        }
#else
        return (null, null);
#endif
    }
}
