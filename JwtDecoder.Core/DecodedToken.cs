using System.Text.Json;

namespace JwtDecoder.Core;

public enum TokenKind
{
    /// <summary>Nothing has been entered</summary>
    Empty,
    /// <summary>The input couldn't be decoded</summary>
    Invalid,
    /// <summary>A signed three-part token (JWS)</summary>
    Signed,
    /// <summary>An encrypted five-part token (JWE), whose payload can't be read</summary>
    Encrypted
}

/// <summary>
/// The result of decoding whatever the user pasted
/// </summary>
public sealed record DecodedToken
{
    public TokenKind Kind { get; init; }

    /// <summary>The token after Bearer / whitespace stripping</summary>
    public string Token { get; init; } = string.Empty;

    public string? Error { get; init; }

    /// <summary>Raw decoded header JSON</summary>
    public string? HeaderJson { get; init; }

    /// <summary>Raw decoded payload JSON (null for an encrypted token)</summary>
    public string? PayloadJson { get; init; }

    public string? Algorithm { get; init; }
    public string? Encryption { get; init; }
    public string? KeyId { get; init; }
    public DateTimeOffset? Expires { get; init; }
    public DateTimeOffset? NotBefore { get; init; }

    /// <summary>False when the payload decoded but isn't valid JSON</summary>
    public bool PayloadIsJson { get; init; }

    public bool IsUnsigned => string.Equals(Algorithm, "none", StringComparison.OrdinalIgnoreCase);

    public static readonly DecodedToken Empty = new() { Kind = TokenKind.Empty };

    /// <summary>
    /// Decodes a pasted token, stripping any Bearer prefix, quotes and whitespace first
    /// </summary>
    /// <param name="input">The raw text from the input box</param>
    public static DecodedToken Decode(string? input)
    {
        string token = JwtParser.NormalizeToken(input ?? string.Empty);
        if (token.Length == 0)
        {
            return Empty;
        }

        string[] parts = token.Split('.');
        if (parts.Length != 3 && parts.Length != 5)
        {
            return Invalid(token, "Invalid JWT format. A JWT should have three parts separated by dots (or five for an encrypted JWE).");
        }

        string headerJson;
        try
        {
            headerJson = JwtParser.DecodeBase64Url(parts[0]);
        }
        catch (FormatException)
        {
            return Invalid(token, "The header is not valid Base64URL.");
        }

        string? algorithm = GetString(headerJson, "alg");
        string? keyId = GetString(headerJson, "kid");

        if (parts.Length == 5)
        {
            return new DecodedToken
            {
                Kind = TokenKind.Encrypted,
                Token = token,
                HeaderJson = headerJson,
                Algorithm = algorithm,
                Encryption = GetString(headerJson, "enc"),
                KeyId = keyId
            };
        }

        string payloadJson;
        try
        {
            payloadJson = JwtParser.DecodeBase64Url(parts[1]);
        }
        catch (FormatException)
        {
            return Invalid(token, "The payload is not valid Base64URL.");
        }

        DecodedToken decoded = new()
        {
            Kind = TokenKind.Signed,
            Token = token,
            HeaderJson = headerJson,
            PayloadJson = payloadJson,
            Algorithm = algorithm,
            KeyId = keyId
        };

        try
        {
            using JsonDocument doc = JsonDocument.Parse(payloadJson);
            JsonElement root = doc.RootElement;
            return decoded with
            {
                PayloadIsJson = true,
                Expires = ReadTimestamp(root, "exp"),
                NotBefore = ReadTimestamp(root, "nbf")
            };
        }
        catch (JsonException)
        {
            return decoded;
        }
    }

    private static DecodedToken Invalid(string token, string error) =>
        new() { Kind = TokenKind.Invalid, Token = token, Error = error };

    private static DateTimeOffset? ReadTimestamp(JsonElement root, string claim)
    {
        if (root.ValueKind == JsonValueKind.Object
            && root.TryGetProperty(claim, out JsonElement value)
            && value.ValueKind == JsonValueKind.Number
            && UnixTime.TryFromSeconds(value.GetDouble(), out DateTimeOffset utc))
        {
            return utc;
        }

        return null;
    }

    private static string? GetString(string json, string property)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(json);
            return doc.RootElement.ValueKind == JsonValueKind.Object
                   && doc.RootElement.TryGetProperty(property, out JsonElement value)
                   && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
