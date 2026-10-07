using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using JwtDecoder.Core;

namespace JwtDecoder.Tests;

public class JwtSignatureVerifierTests
{
    private const string Secret = "a-string-secret-at-least-256-bits-long";

    [Test]
    public void Hs256_WithCorrectSecret_IsValid()
    {
        string token = TestTokens.Hmac("HS256", Secret);

        VerificationResult result = JwtSignatureVerifier.Verify(token, "HS256", null, Secret, false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Valid));
    }

    [Test]
    public void Hs256_IgnoresTrailingLineBreakFromPasting()
    {
        string token = TestTokens.Hmac("HS256", Secret);

        VerificationResult result = JwtSignatureVerifier.Verify(token, "HS256", null, Secret + "\r\n", false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Valid));
    }

    [Test]
    public void Hs256_WithWrongSecret_IsInvalid()
    {
        string token = TestTokens.Hmac("HS256", Secret);

        VerificationResult result = JwtSignatureVerifier.Verify(token, "HS256", null, "wrong", false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Invalid));
    }

    [Test]
    public void Hs256_WithBase64UrlSecret_IsValid()
    {
        string token = TestTokens.Hmac("HS256", Secret);

        VerificationResult result = JwtSignatureVerifier.Verify(token, "HS256", null, TestTokens.Base64Url(Encoding.UTF8.GetBytes(Secret)), true);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Valid));
    }

    [Test]
    public void Hs256_WithShortSecret_WarnsItIsWeak()
    {
        string token = TestTokens.Hmac("HS256", "short");

        VerificationResult result = JwtSignatureVerifier.Verify(token, "HS256", null, "short", false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Warning));
        Assert.That(result.Message, Does.Contain("weak secret"));
    }

    [Test]
    public void Hs256_WithOctJwk_IsValid()
    {
        string token = TestTokens.Hmac("HS256", Secret);
        string jwk = $"{{\"kty\":\"oct\",\"k\":\"{TestTokens.Base64Url(Encoding.UTF8.GetBytes(Secret))}\"}}";

        VerificationResult result = JwtSignatureVerifier.Verify(token, "HS256", null, jwk, false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Valid));
    }

    [TestCase(RsaKeyFormat.SubjectPublicKeyInfoPem)]
    [TestCase(RsaKeyFormat.RsaPublicKeyPem)]
    [TestCase(RsaKeyFormat.BareBase64)]
    [TestCase(RsaKeyFormat.CertificatePem)]
    public void Rs256_WithMatchingPublicKey_IsValid(RsaKeyFormat format)
    {
        using RSA rsa = RSA.Create(2048);
        string token = TestTokens.Rsa("RS256", rsa, RSASignaturePadding.Pkcs1);

        VerificationResult result = JwtSignatureVerifier.Verify(token, "RS256", null, ExportRsaKey(rsa, format), false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Valid), result.Message);
    }

    [Test]
    public void Rs256_WithDifferentKey_IsInvalid()
    {
        using RSA rsa = RSA.Create(2048);
        using RSA other = RSA.Create(2048);
        string token = TestTokens.Rsa("RS256", rsa, RSASignaturePadding.Pkcs1);

        VerificationResult result = JwtSignatureVerifier.Verify(token, "RS256", null, other.ExportSubjectPublicKeyInfoPem(), false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Invalid));
    }

    [Test]
    public void Ps256_WithMatchingPublicKey_IsValid()
    {
        using RSA rsa = RSA.Create(2048);
        string token = TestTokens.Rsa("PS256", rsa, RSASignaturePadding.Pss);

        VerificationResult result = JwtSignatureVerifier.Verify(token, "PS256", null, rsa.ExportSubjectPublicKeyInfoPem(), false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Valid));
    }

    [Test]
    public void Rs256_WithJwks_PicksKeyByKid()
    {
        using RSA rsa = RSA.Create(2048);
        using RSA other = RSA.Create(2048);
        string token = TestTokens.Rsa("RS256", rsa, RSASignaturePadding.Pkcs1, kid: "key2");
        string jwks = $"{{\"keys\":[{RsaJwk(other, "key1")},{RsaJwk(rsa, "key2")}]}}";

        VerificationResult result = JwtSignatureVerifier.Verify(token, "RS256", "key2", jwks, false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Valid));
    }

    [Test]
    public void Rs256_WithJwksMissingKid_ExplainsWhy()
    {
        using RSA rsa = RSA.Create(2048);
        using RSA other = RSA.Create(2048);
        string token = TestTokens.Rsa("RS256", rsa, RSASignaturePadding.Pkcs1, kid: "key9");
        string jwks = $"{{\"keys\":[{RsaJwk(other, "key1")},{RsaJwk(rsa, "key2")}]}}";

        VerificationResult result = JwtSignatureVerifier.Verify(token, "RS256", "key9", jwks, false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Invalid));
        Assert.That(result.Message, Does.Contain("kid \"key9\""));
    }

    [Test]
    public void Rs256_WithUnreadableKey_ExplainsAcceptedFormats()
    {
        using RSA rsa = RSA.Create(2048);
        string token = TestTokens.Rsa("RS256", rsa, RSASignaturePadding.Pkcs1);

        VerificationResult result = JwtSignatureVerifier.Verify(token, "RS256", null, "not a key", false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Invalid));
        Assert.That(result.Message, Does.Contain("expected a PEM public key"));
    }

    [Test]
    public void Es256_WithPemAndJwk_IsValid()
    {
        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        string token = TestTokens.Ecdsa("ES256", ecdsa, HashAlgorithmName.SHA256);
        ECParameters parameters = ecdsa.ExportParameters(false);
        string jwk = $"{{\"kty\":\"EC\",\"crv\":\"P-256\",\"x\":\"{TestTokens.Base64Url(parameters.Q.X!)}\",\"y\":\"{TestTokens.Base64Url(parameters.Q.Y!)}\"}}";

        VerificationResult fromPem = JwtSignatureVerifier.Verify(token, "ES256", null, ecdsa.ExportSubjectPublicKeyInfoPem(), false);
        VerificationResult fromJwk = JwtSignatureVerifier.Verify(token, "ES256", null, jwk, false);

        Assert.That(fromPem.Status, Is.EqualTo(VerificationStatus.Valid));
        Assert.That(fromJwk.Status, Is.EqualTo(VerificationStatus.Valid));
    }

    [Test]
    public void Es384_WithMatchingPublicKey_IsValid()
    {
        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        string token = TestTokens.Ecdsa("ES384", ecdsa, HashAlgorithmName.SHA384);

        VerificationResult result = JwtSignatureVerifier.Verify(token, "ES384", null, ecdsa.ExportSubjectPublicKeyInfoPem(), false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Valid));
    }

    [Test]
    public void Es256_WithTamperedPayload_IsInvalid()
    {
        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        string[] parts = TestTokens.Ecdsa("ES256", ecdsa, HashAlgorithmName.SHA256).Split('.');
        string tampered = $"{parts[0]}.{TestTokens.Base64Url(Encoding.UTF8.GetBytes("{\"sub\":\"admin\"}"))}.{parts[2]}";

        VerificationResult result = JwtSignatureVerifier.Verify(tampered, "ES256", null, ecdsa.ExportSubjectPublicKeyInfoPem(), false);

        Assert.That(result.Status, Is.EqualTo(VerificationStatus.Invalid));
    }

    public enum RsaKeyFormat { SubjectPublicKeyInfoPem, RsaPublicKeyPem, BareBase64, CertificatePem }

    private static string ExportRsaKey(RSA rsa, RsaKeyFormat format)
    {
        switch (format)
        {
            case RsaKeyFormat.SubjectPublicKeyInfoPem:
                return rsa.ExportSubjectPublicKeyInfoPem();
            case RsaKeyFormat.RsaPublicKeyPem:
                return rsa.ExportRSAPublicKeyPem();
            case RsaKeyFormat.BareBase64:
                return Convert.ToBase64String(rsa.ExportSubjectPublicKeyInfo());
            default:
                CertificateRequest request = new("CN=test", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                using (X509Certificate2 certificate = request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1)))
                {
                    return certificate.ExportCertificatePem();
                }
        }
    }

    private static string RsaJwk(RSA rsa, string kid)
    {
        RSAParameters parameters = rsa.ExportParameters(false);
        return $"{{\"kty\":\"RSA\",\"kid\":\"{kid}\",\"n\":\"{TestTokens.Base64Url(parameters.Modulus!)}\",\"e\":\"{TestTokens.Base64Url(parameters.Exponent!)}\"}}";
    }
}
