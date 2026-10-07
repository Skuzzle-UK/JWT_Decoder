using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace JwtDecoder.Core;

/// <summary>
/// A hover / tap hint shown on a claim name or value
/// </summary>
public sealed record Hint(string Title, string Body, bool IsDanger = false);

/// <summary>
/// Works out which claim names and values get hints, and what they say
/// </summary>
public static class ClaimHints
{
    private static readonly Dictionary<string, string> TimeClaims = new()
    {
        ["exp"] = "Expires",
        ["iat"] = "Issued at",
        ["nbf"] = "Not before",
        ["auth_time"] = "Authenticated at",
        ["updated_at"] = "Updated at",
    };

    /// <summary>
    /// The hint for a claim or header parameter name, if it's a well-known one
    /// </summary>
    public static Hint? ForKey(string key)
    {
        return ClaimDescriptions.TryGetClaim(key, out string name, out string description)
            ? new Hint(name, description)
            : null;
    }

    /// <summary>
    /// The hint for a claim value: readable times for timestamps, descriptions for alg / enc
    /// </summary>
    public static Hint? ForValue(string key, JsonNode? value, DateTimeOffset now)
    {
        if (value is not JsonValue jsonValue)
        {
            return null;
        }

        JsonValueKind kind = jsonValue.GetValueKind();

        if (kind == JsonValueKind.Number
            && TimeClaims.TryGetValue(key, out string? label)
            && double.TryParse(jsonValue.ToJsonString(), NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds)
            && UnixTime.TryFromSeconds(seconds, out DateTimeOffset utc))
        {
            DateTimeOffset local = utc.ToLocalTime();
            return new Hint(
                $"{label} ({UnixTime.FormatRelative(utc, now)})",
                $"UTC:    {utc:yyyy-MM-dd HH:mm:ss}\nLocal:  {local:yyyy-MM-dd HH:mm:ss} ({UnixTime.LocalZoneName(utc)})");
        }

        if (kind == JsonValueKind.String && key is "alg" or "enc")
        {
            string algorithm = jsonValue.GetValue<string>();
            if (string.Equals(algorithm, "none", StringComparison.OrdinalIgnoreCase))
            {
                return new Hint(
                    "⚠ Unsigned token",
                    "There is no signature, so anyone can create or modify this token. An API must never accept it.",
                    IsDanger: true);
            }

            string? description = ClaimDescriptions.DescribeAlgorithm(algorithm);
            return description == null ? null : new Hint(algorithm, description);
        }

        return null;
    }

    /// <summary>
    /// The text to copy for a claim value: strings without quotes, anything else as JSON
    /// </summary>
    public static string CopyText(JsonNode? value)
    {
        return value switch
        {
            null => "null",
            JsonValue v when v.GetValueKind() == JsonValueKind.String => v.GetValue<string>(),
            _ => value.ToJsonString(new JsonSerializerOptions { WriteIndented = true })
        };
    }
}
