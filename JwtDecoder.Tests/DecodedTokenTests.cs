using System.Text.Json.Nodes;
using JwtDecoder.Core;

namespace JwtDecoder.Tests;

public class DecodedTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 7, 12, 0, 0, TimeSpan.Zero);

    [TestCase("Bearer {0}")]
    [TestCase("bearer {0}")]
    [TestCase("Authorization: Bearer {0}")]
    [TestCase("  \"{0}\"  ")]
    [TestCase("{0}\r\n")]
    public void Decode_StripsBearerPrefixQuotesAndWhitespace(string format)
    {
        string token = TestTokens.Hmac("HS256", "secret");

        DecodedToken decoded = DecodedToken.Decode(string.Format(format, token));

        Assert.That(decoded.Kind, Is.EqualTo(TokenKind.Signed));
        Assert.That(decoded.Token, Is.EqualTo(token));
    }

    [Test]
    public void Decode_TokenSplitAcrossLines_IsJoined()
    {
        string token = TestTokens.Hmac("HS256", "secret");

        DecodedToken decoded = DecodedToken.Decode(token[..20] + "\n" + token[20..]);

        Assert.That(decoded.Token, Is.EqualTo(token));
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Decode_EmptyInput_IsEmpty(string? input)
    {
        Assert.That(DecodedToken.Decode(input).Kind, Is.EqualTo(TokenKind.Empty));
    }

    [TestCase("abc")]
    [TestCase("a.b")]
    [TestCase("a.b.c.d")]
    public void Decode_WrongNumberOfParts_IsInvalid(string input)
    {
        DecodedToken decoded = DecodedToken.Decode(input);

        Assert.That(decoded.Kind, Is.EqualTo(TokenKind.Invalid));
        Assert.That(decoded.Error, Does.Contain("three parts"));
    }

    [Test]
    public void Decode_ReadsAlgorithmKidAndTimes()
    {
        string token = $"{TestTokens.Base64Url("{\"alg\":\"RS256\",\"kid\":\"abc\"}")}.{TestTokens.Base64Url("{\"exp\":1791374400,\"nbf\":1791370800}")}.sig";

        DecodedToken decoded = DecodedToken.Decode(token);

        Assert.That(decoded.Algorithm, Is.EqualTo("RS256"));
        Assert.That(decoded.KeyId, Is.EqualTo("abc"));
        Assert.That(decoded.Expires, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1791374400)));
        Assert.That(decoded.NotBefore, Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1791370800)));
    }

    [Test]
    public void Decode_FivePartToken_IsEncryptedWithHeaderOnly()
    {
        string header = TestTokens.Base64Url("{\"alg\":\"RSA-OAEP\",\"enc\":\"A256GCM\"}");

        DecodedToken decoded = DecodedToken.Decode($"{header}.key.iv.ciphertext.tag");

        Assert.That(decoded.Kind, Is.EqualTo(TokenKind.Encrypted));
        Assert.That(decoded.Algorithm, Is.EqualTo("RSA-OAEP"));
        Assert.That(decoded.Encryption, Is.EqualTo("A256GCM"));
        Assert.That(decoded.PayloadJson, Is.Null);
    }

    [Test]
    public void Status_ExpiredToken_IsBad()
    {
        DecodedToken decoded = DecodedWithTimes(exp: Now.AddHours(-3));

        TokenStatus? status = TokenStatus.Evaluate(decoded, Now);

        Assert.That(status!.Severity, Is.EqualTo(StatusSeverity.Bad));
        Assert.That(status.Text, Does.StartWith("✖ Expired 3h 0m ago"));
    }

    [Test]
    public void Status_ExpiringWithinFiveMinutes_IsWarning()
    {
        DecodedToken decoded = DecodedWithTimes(exp: Now.AddMinutes(4));

        TokenStatus? status = TokenStatus.Evaluate(decoded, Now);

        Assert.That(status!.Severity, Is.EqualTo(StatusSeverity.Warning));
        Assert.That(status.Text, Does.Contain("Expires soon"));
    }

    [Test]
    public void Status_NotExpired_IsGood()
    {
        DecodedToken decoded = DecodedWithTimes(exp: Now.AddMinutes(42));

        TokenStatus? status = TokenStatus.Evaluate(decoded, Now);

        Assert.That(status!.Severity, Is.EqualTo(StatusSeverity.Good));
        Assert.That(status.Text, Does.Contain("expires in 42m 0s"));
    }

    [Test]
    public void Status_NotBeforeInFuture_IsWarning()
    {
        DecodedToken decoded = DecodedWithTimes(exp: Now.AddHours(2), nbf: Now.AddMinutes(10));

        TokenStatus? status = TokenStatus.Evaluate(decoded, Now);

        Assert.That(status!.Severity, Is.EqualTo(StatusSeverity.Warning));
        Assert.That(status.Text, Does.Contain("Not valid yet"));
    }

    [Test]
    public void Status_NoExpiry_IsWarning()
    {
        DecodedToken decoded = DecodedWithTimes(exp: null);

        Assert.That(TokenStatus.Evaluate(decoded, Now)!.Text, Does.Contain("No expiry"));
    }

    [Test]
    public void Status_UnsignedToken_IsDowngradedToWarning()
    {
        string payload = $"{{\"exp\":{Now.AddHours(1).ToUnixTimeSeconds()}}}";
        DecodedToken decoded = DecodedToken.Decode(TestTokens.Unsigned("{\"alg\":\"none\"}", payload));

        TokenStatus? status = TokenStatus.Evaluate(decoded, Now);

        Assert.That(status!.Severity, Is.EqualTo(StatusSeverity.Warning));
        Assert.That(status.Text, Does.Contain("unsigned"));
    }

    [Test]
    public void Hints_TimestampValue_ShowsUtcAndRelativeTime()
    {
        JsonNode value = JsonValue.Create(Now.AddMinutes(30).ToUnixTimeSeconds());

        Hint? hint = ClaimHints.ForValue("exp", value, Now);

        Assert.That(hint!.Title, Is.EqualTo("Expires (in 30m 0s)"));
        Assert.That(hint.Body, Does.Contain("UTC:    2026-10-07 12:30:00"));
    }

    [Test]
    public void Hints_AlgNone_IsDanger()
    {
        Hint? hint = ClaimHints.ForValue("alg", JsonValue.Create("none"), Now);

        Assert.That(hint!.IsDanger, Is.True);
    }

    [TestCase("HS256", "HMAC with SHA-256")]
    [TestCase("ES512", "P-521")]
    [TestCase("RSA-OAEP-256", "RSA-OAEP (SHA-256)")]
    public void Hints_Algorithm_IsDescribed(string algorithm, string expected)
    {
        Hint? hint = ClaimHints.ForValue("alg", JsonValue.Create(algorithm), Now);

        Assert.That(hint!.Body, Does.Contain(expected));
    }

    [Test]
    public void Hints_KnownAndUnknownKeys()
    {
        Assert.That(ClaimHints.ForKey("aud")!.Title, Is.EqualTo("Audience"));
        Assert.That(ClaimHints.ForKey("my_custom_claim"), Is.Null);
    }

    [Test]
    public void CopyText_StringsAreUnquoted()
    {
        Assert.That(ClaimHints.CopyText(JsonValue.Create("abc")), Is.EqualTo("abc"));
        Assert.That(ClaimHints.CopyText(JsonValue.Create(42)), Is.EqualTo("42"));
    }

    private static DecodedToken DecodedWithTimes(DateTimeOffset? exp, DateTimeOffset? nbf = null)
    {
        JsonObject payload = new();
        if (exp != null)
        {
            payload["exp"] = exp.Value.ToUnixTimeSeconds();
        }
        if (nbf != null)
        {
            payload["nbf"] = nbf.Value.ToUnixTimeSeconds();
        }

        return DecodedToken.Decode(TestTokens.Hmac("HS256", "secret", payload.ToJsonString()));
    }
}
