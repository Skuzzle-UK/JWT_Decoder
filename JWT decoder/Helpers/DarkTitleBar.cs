using System.Runtime.InteropServices;

namespace JWT_decoder.Helpers;

public static class DarkTitleBar
{
    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attr, ref int attrValue, int attrSize);

    private const int DARK_MODE_BEFORE_20H1 = 19;
    private const int DARK_MODE = 20;

    /// <summary>
    /// Enables dark mode title bar for a form on Windows 10/11
    /// </summary>
    /// <param name="form">The form to apply dark title bar to</param>
    public static void ApplyTo(Form form)
    {
        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            int preference = 1;
            // Try newer API first (Windows 11 / Windows 10 20H1+)
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 19041))
            {
                DwmSetWindowAttribute(form.Handle, DARK_MODE, ref preference, sizeof(int));
            }
            else
            {
                // Fallback for older Windows 10 versions
                DwmSetWindowAttribute(form.Handle, DARK_MODE_BEFORE_20H1, ref preference, sizeof(int));
            }
        }
    }
}
