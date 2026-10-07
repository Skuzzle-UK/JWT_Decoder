using System.Text.RegularExpressions;

namespace JwtDecoder.Core;

/// <summary>
/// Plain-English descriptions of common JWT header parameters, claims and algorithms
/// </summary>
public static class ClaimDescriptions
{
    private const string XmlSoapClaims = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/";
    private const string MicrosoftClaims = "http://schemas.microsoft.com/ws/2008/06/identity/claims/";

    private static readonly Dictionary<string, (string Name, string Description)> Claims = new()
    {
        // Header parameters
        ["alg"] = ("Algorithm", "How the token is signed (JWS) or how the content key is encrypted (JWE)."),
        ["typ"] = ("Type", "Media type of the token, usually \"JWT\", or \"at+jwt\" for OAuth access tokens."),
        ["kid"] = ("Key ID", "Which of the issuer's keys signed the token. Look it up in the issuer's JWKS."),
        ["cty"] = ("Content Type", "Set to \"JWT\" when the payload is itself a nested JWT."),
        ["x5t"] = ("X.509 Thumbprint", "SHA-1 thumbprint of the certificate whose key signed the token."),
        ["x5t#S256"] = ("X.509 Thumbprint (SHA-256)", "SHA-256 thumbprint of the certificate whose key signed the token."),
        ["x5c"] = ("X.509 Certificate Chain", "The certificate chain for the signing key, embedded in the token."),
        ["x5u"] = ("X.509 URL", "URL of the certificate chain for the signing key."),
        ["jku"] = ("JWK Set URL", "URL of the key set containing the signing key. Never trust this blindly."),
        ["jwk"] = ("JSON Web Key", "The signing public key embedded in the token. Never trust this blindly."),
        ["enc"] = ("Encryption", "Content encryption algorithm used for the payload (JWE only)."),
        ["zip"] = ("Compression", "Compression applied to the payload before encryption (JWE only)."),
        ["crit"] = ("Critical", "Header parameters the receiver must understand, or reject the token."),

        // Registered claims (RFC 7519)
        ["iss"] = ("Issuer", "Who created and signed the token, usually the identity provider's URL."),
        ["sub"] = ("Subject", "The user or client the token is about. Unique within the issuer."),
        ["aud"] = ("Audience", "Who the token is intended for. An API should reject tokens whose audience isn't its own."),
        ["exp"] = ("Expiration Time", "After this time the token must be rejected."),
        ["nbf"] = ("Not Before", "Before this time the token must be rejected."),
        ["iat"] = ("Issued At", "When the token was created."),
        ["jti"] = ("JWT ID", "Unique ID for this token. Can be used to detect replayed tokens."),

        // OAuth 2.0 / OpenID Connect
        ["azp"] = ("Authorized Party", "The client application the token was issued to."),
        ["client_id"] = ("Client ID", "The client application the token was issued to."),
        ["scope"] = ("Scope", "Permissions granted to the client, space-separated."),
        ["scp"] = ("Scopes", "Delegated permissions granted to the app on behalf of the user, space-separated."),
        ["roles"] = ("Roles", "Roles assigned to the user or application."),
        ["role"] = ("Role", "Role(s) assigned to the user."),
        ["groups"] = ("Groups", "Groups the user is a member of."),
        ["auth_time"] = ("Authentication Time", "When the user actually signed in, which can be earlier than iat."),
        ["nonce"] = ("Nonce", "Value from the sign-in request, used to stop ID tokens being replayed."),
        ["amr"] = ("Authentication Methods", "How the user signed in, e.g. pwd, mfa, otp."),
        ["acr"] = ("Authentication Context", "The assurance level of the sign-in."),
        ["sid"] = ("Session ID", "The sign-in session the token belongs to."),
        ["at_hash"] = ("Access Token Hash", "Hash of the access token issued alongside this ID token."),
        ["c_hash"] = ("Code Hash", "Hash of the authorization code issued alongside this ID token."),
        ["idp"] = ("Identity Provider", "The provider the user originally signed in with."),
        ["cnf"] = ("Confirmation", "Proof-of-possession key the caller must prove it holds."),
        ["act"] = ("Actor", "The party acting on behalf of the subject (delegation / token exchange)."),
        ["name"] = ("Name", "The user's full display name."),
        ["given_name"] = ("Given Name", "The user's first name."),
        ["family_name"] = ("Family Name", "The user's surname."),
        ["email"] = ("Email", "The user's email address."),
        ["email_verified"] = ("Email Verified", "Whether the identity provider has verified the email address."),
        ["preferred_username"] = ("Preferred Username", "The username the user signs in with. Not guaranteed unique or stable."),
        ["updated_at"] = ("Updated At", "When the user's profile was last changed."),

        // Microsoft Entra ID (Azure AD)
        ["oid"] = ("Object ID", "Immutable ID of the user or app in the Microsoft Entra tenant."),
        ["tid"] = ("Tenant ID", "The Microsoft Entra tenant the user or app belongs to."),
        ["upn"] = ("User Principal Name", "The user's sign-in name. Can change, so don't use it as a key."),
        ["appid"] = ("Application ID", "The client application the token was issued to (v1.0 tokens)."),
        ["ver"] = ("Version", "Token format version, 1.0 or 2.0."),

        // ASP.NET / WS-Federation claim types
        [XmlSoapClaims + "nameidentifier"] = ("Name Identifier", "ClaimTypes.NameIdentifier, the user's unique ID. ASP.NET maps \"sub\" to this."),
        [XmlSoapClaims + "name"] = ("Name", "ClaimTypes.Name, used for User.Identity.Name."),
        [XmlSoapClaims + "emailaddress"] = ("Email", "ClaimTypes.Email, the user's email address."),
        [XmlSoapClaims + "givenname"] = ("Given Name", "ClaimTypes.GivenName, the user's first name."),
        [XmlSoapClaims + "surname"] = ("Surname", "ClaimTypes.Surname, the user's last name."),
        [XmlSoapClaims + "upn"] = ("User Principal Name", "ClaimTypes.Upn, the user's sign-in name."),
        [MicrosoftClaims + "role"] = ("Role", "ClaimTypes.Role, used by [Authorize(Roles = ...)] and User.IsInRole."),
    };

    private static readonly Dictionary<string, string> EncryptionAlgorithms = new()
    {
        ["RSA-OAEP"] = "Content key encrypted with RSA-OAEP (SHA-1). Decrypt with the recipient's private key.",
        ["RSA-OAEP-256"] = "Content key encrypted with RSA-OAEP (SHA-256). Decrypt with the recipient's private key.",
        ["RSA1_5"] = "Content key encrypted with RSA PKCS#1 v1.5. Considered weak; prefer RSA-OAEP.",
        ["dir"] = "Direct encryption with a shared symmetric key.",
        ["A128KW"] = "Content key wrapped with AES-128 Key Wrap and a shared key.",
        ["A192KW"] = "Content key wrapped with AES-192 Key Wrap and a shared key.",
        ["A256KW"] = "Content key wrapped with AES-256 Key Wrap and a shared key.",
        ["ECDH-ES"] = "Content key agreed with Elliptic Curve Diffie-Hellman.",
        ["A128GCM"] = "Payload encrypted with AES-128 in GCM mode.",
        ["A192GCM"] = "Payload encrypted with AES-192 in GCM mode.",
        ["A256GCM"] = "Payload encrypted with AES-256 in GCM mode.",
        ["A128CBC-HS256"] = "Payload encrypted with AES-128-CBC and authenticated with HMAC SHA-256.",
        ["A192CBC-HS384"] = "Payload encrypted with AES-192-CBC and authenticated with HMAC SHA-384.",
        ["A256CBC-HS512"] = "Payload encrypted with AES-256-CBC and authenticated with HMAC SHA-512.",
    };

    private static readonly Regex SignatureAlgorithmRegex = new(@"^(HS|RS|PS|ES)(256|384|512)$");

    /// <summary>
    /// Looks up a description for a header parameter or claim name
    /// </summary>
    public static bool TryGetClaim(string key, out string name, out string description)
    {
        if (Claims.TryGetValue(key, out (string Name, string Description) claim))
        {
            (name, description) = claim;
            return true;
        }

        name = description = string.Empty;
        return false;
    }

    /// <summary>
    /// Describes an alg or enc header value (other than "none", which isn't an algorithm)
    /// </summary>
    public static string? DescribeAlgorithm(string algorithm)
    {
        if (algorithm == "EdDSA")
        {
            return "Edwards-curve signature (Ed25519 / Ed448).\nVerify with the issuer's public key.";
        }

        if (EncryptionAlgorithms.TryGetValue(algorithm, out string? encryption))
        {
            return encryption;
        }

        Match match = SignatureAlgorithmRegex.Match(algorithm);
        if (!match.Success)
        {
            return null;
        }

        string bits = match.Groups[2].Value;
        return match.Groups[1].Value switch
        {
            "HS" => $"HMAC with SHA-{bits}.\nSymmetric: the same shared secret signs and verifies the token.",
            "RS" => $"RSA PKCS#1 v1.5 signature with SHA-{bits}.\nVerify with the issuer's public key.",
            "PS" => $"RSA-PSS signature with SHA-{bits}.\nVerify with the issuer's public key.",
            _ => $"ECDSA signature using {(bits == "512" ? "P-521" : $"P-{bits}")} and SHA-{bits}.\nVerify with the issuer's public key."
        };
    }
}
