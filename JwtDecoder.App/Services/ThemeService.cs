namespace JwtDecoder.App.Services;

public enum ThemeChoice { System, Light, Dark }

/// <summary>
/// The user's light / dark choice: saved between launches, and applied to both the native window and the page
/// </summary>
public sealed class ThemeService
{
    private const string PreferenceKey = "theme";

    public ThemeService()
    {
        Choice = LoadChoice();

        // Fires when the OS theme changes (relevant when following the system) and when UserAppTheme changes
        if (Application.Current is { } app)
        {
            app.RequestedThemeChanged += (_, _) => Changed?.Invoke();
        }
    }

    /// <summary>Raised when the effective theme may have changed</summary>
    public event Action? Changed;

    public ThemeChoice Choice { get; private set; }

    /// <summary>The theme actually in use: "light" or "dark"</summary>
    public string Resolved => Application.Current?.RequestedTheme == AppTheme.Dark ? "dark" : "light";

    public void SetChoice(ThemeChoice choice)
    {
        Choice = choice;
        Preferences.Default.Set(PreferenceKey, choice.ToString());
        Apply(choice);
        Changed?.Invoke();
    }

    /// <summary>
    /// Applies the saved choice to the app. Called at startup so the window opens in the right theme.
    /// </summary>
    public static void ApplySaved() => Apply(LoadChoice());

    private static ThemeChoice LoadChoice() =>
        Enum.TryParse(Preferences.Default.Get(PreferenceKey, nameof(ThemeChoice.System)), out ThemeChoice choice)
            ? choice
            : ThemeChoice.System;

    private static void Apply(ThemeChoice choice)
    {
        if (Application.Current is { } app)
        {
            app.UserAppTheme = choice switch
            {
                ThemeChoice.Light => AppTheme.Light,
                ThemeChoice.Dark => AppTheme.Dark,
                _ => AppTheme.Unspecified
            };
        }
    }
}
