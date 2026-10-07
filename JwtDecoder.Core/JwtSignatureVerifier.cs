using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace JwtDecoder.Core;

public enum VerificationStatus { Valid, Warning, Invalid }

public sealed record VerificationResult(VerificationStatus Status, string Message);

/// <summary>
/// Verifies JWT signatures offline with a shared secret (HS*) or a public key (RS*, PS*, ES*)
/// </summary>
public static class JwtSignatureVerifier
{
    /// <summary>
    /// True for algorithms verified with a shared secret rather than a public key
    /// </summary>
    public static bool IsSymmetric(string? algorithm) => algorithm is "HS256" or "HS384" or "HS512";

    /// <summary>
    /// True for algorithms this verifier can check
    /// </summary>
    public static bool IsSupported(string? algorithm) => algorithm is
        "HS256" or "HS384" or "HS512" or
        "RS256" or "RS384" or "RS512" or
        "PS256" or "PS384" or "PS512" or
        "ES256" or "ES384" or "ES512";

    /// <summary>
    /// Verifies the signature of a three-part JWT
    /// </summary>
    /// <param name="token">The token (header.payload.signature)</param>
    /// <param name="algorithm">The alg header value</param>
    /// <param name="keyId">The kid header value, used to pick a key from a JWKS</param>
    /// <param name="key">A secret for HS*, or a PEM / certificate / JWK / JWKS public key otherwise</param>
    /// <param name="secretIsBase64Url">Whether an HS* secret is Base64URL encoded rather than plain text</param>
    public static VerificationResult Verify(string token, string algorithm, string? keyId, string key, bool secretIsBase64Url)
    {
        string[] parts = token.Split('.');
        byte[] signingInput = Encoding.ASCII.GetBytes($"{parts[0]}.{parts[1]}");

        byte[] signature;
        try
        {
            signature = JwtParser.DecodeBase64UrlBytes(parts[2]);
        }
        catch (FormatException)
        {
            return new(VerificationStatus.Invalid, "✖ Invalid signature: the signature part is not valid Base64URL");
        }

        HashAlgorithmName hash = algorithm[2..] switch
        {
            "256" => HashAlgorithmName.SHA256,
            "384" => HashAlgorithmName.SHA384,
            _ => HashAlgorithmName.SHA512
        };

        try
        {
            return algorithm[..2] switch
            {
                "HS" => VerifyHmac(hash, signingInput, signature, key, secretIsBase64Url),
                "RS" => VerifyWithKeys(LoadKeys(key, keyId, "RSA", RsaFromJwk, cert => cert.GetRSAPublicKey(), RSA.Create),
                    rsa => rsa.VerifyData(signingInput, signature, hash, RSASignaturePadding.Pkcs1)),
                "PS" => VerifyWithKeys(LoadKeys(key, keyId, "RSA", RsaFromJwk, cert => cert.GetRSAPublicKey(), RSA.Create),
                    rsa => rsa.VerifyData(signingInput, signature, hash, RSASignaturePadding.Pss)),
                _ => VerifyWithKeys(LoadKeys(key, keyId, "EC", EcdsaFromJwk, cert => cert.GetECDsaPublicKey(), ECDsa.Create),
                    ecdsa => ecdsa.VerifyData(signingInput, signature, hash)) // JWT uses the IEEE P1363 (r||s) format, which is the .NET default
            };
        }
        catch (Exception ex) when (ex is CryptographicException or FormatException or JsonException or ArgumentException or InvalidOperationException)
        {
            return new(VerificationStatus.Invalid, $"✖ Couldn't use the key: {ex.Message}");
        }
    }

    private static VerificationResult VerifyHmac(HashAlgorithmName hash, byte[] signingInput, byte[] signature, string key, bool secretIsBase64Url)
    {
        byte[] secret = ReadSecret(key, secretIsBase64Url);
        byte[] expected = hash.Name switch
        {
            "SHA256" => HMACSHA256.HashData(secret, signingInput),
            "SHA384" => HMACSHA384.HashData(secret, signingInput),
            _ => HMACSHA512.HashData(secret, signingInput)
        };

        if (!CryptographicOperations.FixedTimeEquals(expected, signature))
        {
            return new(VerificationStatus.Invalid, "✖ Invalid signature");
        }

        // RFC 7518 requires the secret to be at least as long as the hash output
        int minimumBits = expected.Length * 8;
        int secretBits = secret.Length * 8;
        return secretBits < minimumBits
            ? new(VerificationStatus.Warning, $"✔ Signature verified  ·  ⚠ weak secret: {secretBits} bits, should be at least {minimumBits}")
            : new(VerificationStatus.Valid, "✔ Signature verified");
    }

    private static byte[] ReadSecret(string key, bool secretIsBase64Url)
    {
        // Accept a symmetric JWK ({"kty":"oct","k":"..."}) as well as a raw secret
        if (key.TrimStart().StartsWith('{'))
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(key);
                if (GetString(doc.RootElement, "kty") == "oct" && GetString(doc.RootElement, "k") is string k)
                {
                    return JwtParser.DecodeBase64UrlBytes(k);
                }
            }
            catch (JsonException)
            {
                // Not a JWK, so treat it as a plain secret
            }
        }

        // Ignore a trailing line break from pasting, but keep any other whitespace as it's part of the secret
        string secret = key.TrimEnd('\r', '\n');
        return secretIsBase64Url
            ? JwtParser.DecodeBase64UrlBytes(secret.Trim())
            : Encoding.UTF8.GetBytes(secret);
    }

    private static VerificationResult VerifyWithKeys<T>(List<T> keys, Func<T, bool> verify) where T : AsymmetricAlgorithm
    {
        try
        {
            return keys.Any(verify)
                ? new(VerificationStatus.Valid, "✔ Signature verified")
                : new(VerificationStatus.Invalid, "✖ Invalid signature");
        }
        finally
        {
            keys.ForEach(k => k.Dispose());
        }
    }

    /// <summary>
    /// Reads public keys from a JWK, JWKS, PEM certificate, PEM key or bare Base64 SubjectPublicKeyInfo
    /// </summary>
    private static List<T> LoadKeys<T>(
        string keyText,
        string? keyId,
        string keyType,
        Func<JsonElement, T> fromJwk,
        Func<X509Certificate2, T?> fromCertificate,
        Func<T> create) where T : AsymmetricAlgorithm
    {
        string text = keyText.Trim();

        if (text.StartsWith('{'))
        {
            using JsonDocument doc = JsonDocument.Parse(text);
            JsonElement root = doc.RootElement;
            List<JsonElement> candidates = (root.TryGetProperty("keys", out JsonElement keys) ? keys.EnumerateArray().ToList() : [root])
                .Where(k => GetString(k, "kty") == keyType)
                .ToList();

            if (candidates.Count == 0)
            {
                throw new CryptographicException($"no {keyType} key found in the JWK");
            }

            if (keyId != null && candidates.Count > 1)
            {
                candidates = candidates.Where(k => GetString(k, "kid") == keyId).ToList();
                if (candidates.Count == 0)
                {
                    throw new CryptographicException($"no key in the JWKS has kid \"{keyId}\"");
                }
            }

            return candidates.Select(fromJwk).ToList();
        }

        if (text.Contains("-----BEGIN CERTIFICATE", StringComparison.Ordinal))
        {
            using X509Certificate2 certificate = X509Certificate2.CreateFromPem(text);
            T certificateKey = fromCertificate(certificate)
                ?? throw new CryptographicException($"the certificate doesn't contain an {keyType} key");
            return [certificateKey];
        }

        T key = create();
        if (text.Contains("-----BEGIN", StringComparison.Ordinal))
        {
            key.ImportFromPem(text);
        }
        else
        {
            try
            {
                key.ImportSubjectPublicKeyInfo(Convert.FromBase64String(text), out _);
            }
            catch (Exception ex) when (ex is FormatException or CryptographicException)
            {
                key.Dispose();
                throw new CryptographicException("expected a PEM public key, certificate, JWK or JWKS");
            }
        }

        return [key];
    }

    private static RSA RsaFromJwk(JsonElement jwk)
    {
        return RSA.Create(new RSAParameters
        {
            Modulus = JwtParser.DecodeBase64UrlBytes(RequireString(jwk, "n")),
            Exponent = JwtParser.DecodeBase64UrlBytes(RequireString(jwk, "e"))
        });
    }

    private static ECDsa EcdsaFromJwk(JsonElement jwk)
    {
        ECCurve curve = RequireString(jwk, "crv") switch
        {
            "P-256" => ECCurve.NamedCurves.nistP256,
            "P-384" => ECCurve.NamedCurves.nistP384,
            "P-521" => ECCurve.NamedCurves.nistP521,
            string other => throw new CryptographicException($"unsupported curve \"{other}\"")
        };

        return ECDsa.Create(new ECParameters
        {
            Curve = curve,
            Q = new ECPoint
            {
                X = JwtParser.DecodeBase64UrlBytes(RequireString(jwk, "x")),
                Y = JwtParser.DecodeBase64UrlBytes(RequireString(jwk, "y"))
            }
        });
    }

    private static string? GetString(JsonElement element, string property)
    {
        return element.ValueKind == JsonValueKind.Object
               && element.TryGetProperty(property, out JsonElement value)
               && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string RequireString(JsonElement element, string property)
    {
        return GetString(element, property) ?? throw new CryptographicException($"the JWK is missing \"{property}\"");
    }
}
