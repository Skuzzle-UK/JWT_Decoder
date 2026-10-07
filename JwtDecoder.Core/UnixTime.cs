namespace JwtDecoder.Core;

public static class UnixTime
{
    /// <summary>
    /// Converts a Unix timestamp in seconds (JWT NumericDate) to a DateTimeOffset
    /// </summary>
    /// <returns>False if the value is outside the range a DateTimeOffset can represent</returns>
    public static bool TryFromSeconds(double seconds, out DateTimeOffset utc)
    {
        try
        {
            utc = DateTimeOffset.FromUnixTimeMilliseconds((long)(seconds * 1000));
            return true;
        }
        catch (ArgumentOutOfRangeException)
        {
            utc = default;
            return false;
        }
    }

    /// <summary>
    /// Describes a time relative to now, e.g. "in 42m 10s" or "3h 5m ago"
    /// </summary>
    public static string FormatRelative(DateTimeOffset utc, DateTimeOffset now)
    {
        TimeSpan diff = utc - now;
        string amount = FormatDuration(diff.Duration());
        return diff > TimeSpan.Zero ? $"in {amount}" : $"{amount} ago";
    }

    /// <summary>
    /// Formats a time in local time, including the date only when it isn't today
    /// </summary>
    public static string FormatLocal(DateTimeOffset utc)
    {
        DateTimeOffset local = utc.ToLocalTime();
        return local.Date == DateTime.Today
            ? local.ToString("HH:mm:ss")
            : local.ToString("dd MMM yyyy HH:mm:ss");
    }

    /// <summary>
    /// The name of the local time zone at the given time, e.g. "GMT Summer Time"
    /// </summary>
    public static string LocalZoneName(DateTimeOffset utc)
    {
        TimeZoneInfo zone = TimeZoneInfo.Local;
        return zone.IsDaylightSavingTime(utc) ? zone.DaylightName : zone.StandardName;
    }

    private static string FormatDuration(TimeSpan span)
    {
        if (span.TotalSeconds < 60)
        {
            return $"{(int)span.TotalSeconds}s";
        }

        if (span.TotalHours < 1)
        {
            return $"{span.Minutes}m {span.Seconds}s";
        }

        if (span.TotalDays < 1)
        {
            return $"{span.Hours}h {span.Minutes}m";
        }

        return $"{(int)span.TotalDays}d {span.Hours}h";
    }
}
