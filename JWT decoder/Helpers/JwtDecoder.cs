using System.Text;
using System.Text.Json;

namespace JWT_decoder.Helpers;

public static class JwtDecoder
{
    /// <summary>
    /// Decodes a Base64URL encoded string (used in JWT tokens)
    /// </summary>
    /// <param name="base64Url">The Base64URL encoded string</param>
    /// <returns>The decoded string</returns>
    public static string DecodeBase64Url(string base64Url)
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

        byte[] bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    /// <summary>
    /// Formats JSON string with indentation for better readability
    /// </summary>
    /// <param name="json">The JSON string to format</param>
    /// <returns>Formatted JSON string</returns>
    public static string FormatJson(string json)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
        }
        catch
        {
            return json;
        }
    }
}
