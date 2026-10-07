using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace JwtDecoder.Core;

public static class JwtParser
{
    private static readonly Regex BearerPrefixRegex = new(@"^\s*(authorization\s*:\s*)?bearer\s+", RegexOptions.IgnoreCase);
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    /// <summary>
    /// Cleans up pasted input: strips an "Authorization: Bearer " / "Bearer " prefix,
    /// surrounding quotes and any whitespace or line breaks
    /// </summary>
    /// <param name="input">The raw text from the input box</param>
    /// <returns>The bare token</returns>
    public static string NormalizeToken(string input)
    {
        string token = BearerPrefixRegex.Replace(input, string.Empty);
        token = new string(token.Where(c => !char.IsWhiteSpace(c)).ToArray());
        return token.Trim('"', '\'');
    }

    /// <summary>
    /// Decodes a Base64URL encoded string (used in JWT tokens)
    /// </summary>
    /// <param name="base64Url">The Base64URL encoded string</param>
    /// <returns>The decoded string</returns>
    public static string DecodeBase64Url(string base64Url)
    {
        return Encoding.UTF8.GetString(DecodeBase64UrlBytes(base64Url));
    }

    /// <summary>
    /// Decodes a Base64URL encoded string (used in JWT tokens) to raw bytes
    /// </summary>
    /// <param name="base64Url">The Base64URL encoded string</param>
    /// <returns>The decoded bytes</returns>
    public static byte[] DecodeBase64UrlBytes(string base64Url)
    {
        // Base64URL differs from Base64 in that it uses - instead of + and _ instead of /
        // and doesn't use padding (=)
        string base64 = base64Url.Replace('-', '+').Replace('_', '/');

        // Add padding if necessary
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }

    /// <summary>
    /// Formats JSON string with indentation for better readability
    /// </summary>
    /// <param name="json">The JSON string to format</param>
    /// <returns>Formatted JSON string, or the original string if it isn't valid JSON</returns>
    public static string FormatJson(string json)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, IndentedJson);
        }
        catch (JsonException)
        {
            return json;
        }
    }
}
