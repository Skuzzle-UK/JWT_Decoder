# JWT decoder

A .NET MAUI Blazor Hybrid app for Windows, macOS, iOS and Android that you can paste a JWT bearer token into and see the header and payload decoded.
This is great when developing API's to check that the correct claims exist in the token or for just checking how much longer you have on a tokens expiry time.

This was developed to avoid posting token data onto websites to achieve the same result. Everything happens on the device; nothing is sent anywhere.

## Features

- Decodes as you paste or type. A leading `Bearer ` / `Authorization: Bearer `, quotes and line breaks are stripped automatically.
- Expiry banner showing whether the token is valid, expiring soon or expired, updated every second.
- Hover (or tap on touch screens) for details:
  - `exp`, `iat`, `nbf` and other timestamps show the time in UTC and your local time.
  - Well-known claim names (`aud`, `scp`, `oid`, the ASP.NET `ClaimTypes` URIs, ...) explain what they mean.
  - `alg` / `enc` values describe the algorithm, and `alg: none` is flagged as unsigned.
- Copy the header, the payload, or any single claim value.
- Optional signature verification, offline:
  - HS256/384/512 with a secret (plain text or Base64URL).
  - RS*, PS* and ES* with a public key as PEM, certificate, JWK or JWKS (matched by `kid`).
- Encrypted tokens (JWE) show their header, and explain that the payload can't be read.

## Projects

| Project | What it is |
| --- | --- |
| `JwtDecoder.App` | The MAUI Blazor Hybrid app (UI in `Components/`) |
| `JwtDecoder.Core` | Decoding, expiry status, claim descriptions and signature verification. Plain .NET, no UI |
| `JwtDecoder.Tests` | NUnit tests for `JwtDecoder.Core` |

## Building

Requires the .NET 10 SDK and the MAUI workloads (`dotnet workload install maui`).

```bash
dotnet run --project JwtDecoder.App -f net10.0-windows10.0.19041.0
```

```bash
dotnet test JwtDecoder.Tests
```

- **Windows** builds and runs as an unpackaged app.
- **Android** builds on Windows or Mac; deploy to an emulator or device from Visual Studio.
- **iOS / macOS (Mac Catalyst)** compile on Windows, but running them needs a Mac (paired to Visual Studio, or building on the Mac directly), and a real iPhone also needs an Apple Developer account.
- Before publishing to an app store, change `ApplicationId` in `JwtDecoder.App.csproj` from `com.companyname.jwtdecoder`.

Feel free to use this project for any reason that is considered good and lawful.
I will not be held responsible for any misuse of this project.
