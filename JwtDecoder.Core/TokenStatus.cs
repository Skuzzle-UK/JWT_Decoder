namespace JwtDecoder.Core;

public enum StatusSeverity { Good, Warning, Bad, Info }

/// <summary>
/// The one-line expiry status shown in the banner
/// </summary>
public sealed record TokenStatus(StatusSeverity Severity, string Text)
{
    private static readonly TimeSpan ExpiresSoonThreshold = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Works out the banner status for a decoded token, or null when nothing is decoded
    /// </summary>
    public static TokenStatus? Evaluate(DecodedToken token, DateTimeOffset now)
    {
        switch (token.Kind)
        {
            case TokenKind.Empty:
                return null;
            case TokenKind.Invalid:
                return new(StatusSeverity.Bad, $"✖ {token.Error}");
            case TokenKind.Encrypted:
                return new(StatusSeverity.Info,
                    $"🔒 Encrypted token (JWE)  ·  alg {token.Algorithm ?? "?"}, enc {token.Encryption ?? "?"}  ·  the payload can't be read without the recipient's key");
        }

        if (!token.PayloadIsJson)
        {
            return new(StatusSeverity.Warning, "⚠ Payload is not JSON");
        }

        TokenStatus status = EvaluateTimes(token.Expires, token.NotBefore, now);

        if (token.IsUnsigned)
        {
            status = new(
                status.Severity == StatusSeverity.Good ? StatusSeverity.Warning : status.Severity,
                status.Text + "   ·   ⚠ alg is \"none\": the token is unsigned");
        }

        return status;
    }

    private static TokenStatus EvaluateTimes(DateTimeOffset? expires, DateTimeOffset? notBefore, DateTimeOffset now)
    {
        if (notBefore > now)
        {
            return new(StatusSeverity.Warning,
                $"⚠ Not valid yet  ·  becomes valid {UnixTime.FormatRelative(notBefore.Value, now)} (at {UnixTime.FormatLocal(notBefore.Value)} local)");
        }

        if (expires == null)
        {
            return new(StatusSeverity.Warning, "⚠ No expiry (exp) claim  ·  this token never expires");
        }

        string when = $"{UnixTime.FormatRelative(expires.Value, now)} (at {UnixTime.FormatLocal(expires.Value)} local)";

        if (expires <= now)
        {
            return new(StatusSeverity.Bad, $"✖ Expired {when}");
        }

        if (expires - now < ExpiresSoonThreshold)
        {
            return new(StatusSeverity.Warning, $"⚠ Expires soon  ·  {when}");
        }

        return new(StatusSeverity.Good, $"✔ Not expired  ·  expires {when}");
    }
}
