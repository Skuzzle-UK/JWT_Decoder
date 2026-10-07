using System.Security.Cryptography;
using System.Text;

namespace JwtDecoder.Tests;

/// <summary>
/// Builds signed tokens for tests
/// </summary>
internal static class TestTokens
{
    public const string DefaultPayload = "{\"sub\":\"1234567890\"}";

    public static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    public static string Base64Url(string text) => Base64Url(Encoding.UTF8.GetBytes(text));

    public static string Unsigned(string headerJson, string payloadJson) =>
        $"{Base64Url(headerJson)}.{Base64Url(payloadJson)}.";

    public static string Hmac(string alg, string secret, string payloadJson = DefaultPayload)
    {
        string input = SigningInput(alg, null, payloadJson);
        byte[] key = Encoding.UTF8.GetBytes(secret);
        byte[] data = Encoding.ASCII.GetBytes(input);
        byte[] signature = alg switch
        {
            "HS256" => HMACSHA256.HashData(key, data),
            "HS384" => HMACSHA384.HashData(key, data),
            _ => HMACSHA512.HashData(key, data)
        };
        return $"{input}.{Base64Url(signature)}";
    }

    public static string Rsa(string alg, RSA rsa, RSASignaturePadding padding, string? kid = null)
    {
        string input = SigningInput(alg, kid, DefaultPayload);
        byte[] signature = rsa.SignData(Encoding.ASCII.GetBytes(input), HashAlgorithmName.SHA256, padding);
        return $"{input}.{Base64Url(signature)}";
    }

    public static string Ecdsa(string alg, ECDsa ecdsa, HashAlgorithmName hash)
    {
        string input = SigningInput(alg, null, DefaultPayload);
        byte[] signature = ecdsa.SignData(Encoding.ASCII.GetBytes(input), hash);
        return $"{input}.{Base64Url(signature)}";
    }

    private static string SigningInput(string alg, string? kid, string payloadJson)
    {
        string header = kid == null
            ? $"{{\"alg\":\"{alg}\",\"typ\":\"JWT\"}}"
            : $"{{\"alg\":\"{alg}\",\"typ\":\"JWT\",\"kid\":\"{kid}\"}}";
        return $"{Base64Url(header)}.{Base64Url(payloadJson)}";
    }
}
