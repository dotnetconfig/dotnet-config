﻿// <autogenerated />
#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Devlooped.Sponsors;

static partial class SponsorLink
{
    public static Dictionary<string, string> Sponsorables { get; } = typeof(SponsorLink).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .Where(x => x.Key.StartsWith("Funding.GitHub."))
        .Select(x => new { Key = x.Key[15..], x.Value })
        .ToDictionary(x => x.Key, x => x.Value);

    /// <summary>
    /// Whether the current process is running in an IDE, either 
    /// <see cref="IsVisualStudio"/> or <see cref="IsRider"/>.
    /// </summary>
    public static bool IsEditor => IsVisualStudio || IsRider;

    /// <summary>
    /// Whether the current process is running as part of an active Visual Studio instance.
    /// </summary>
    public static bool IsVisualStudio =>
        Environment.GetEnvironmentVariable("ServiceHubLogSessionKey") != null ||
        Environment.GetEnvironmentVariable("VSAPPIDNAME") != null;

    /// <summary>
    /// Whether the current process is running as part of an active Rider instance.
    /// </summary>
    public static bool IsRider =>
        Environment.GetEnvironmentVariable("RESHARPER_FUS_SESSION") != null ||
        Environment.GetEnvironmentVariable("IDEA_INITIAL_DIRECTORY") != null;

    /// <summary>
    /// Manages the sharing and reporting of diagnostics across the source generator 
    /// and the diagnostic analyzer, to avoid doing the online check more than once.
    /// </summary>
    public static DiagnosticsManager Diagnostics { get; } = new();

    /// <summary>
    /// Gets the expiration date from the principal, if any.
    /// </summary>
    /// <returns>
    /// Whichever "exp" claim is the latest, or <see langword="null"/> if none found.
    /// </returns>
    public static DateTime? GetExpiration(this ClaimsPrincipal principal)
        // get all "exp" claims, parse them and return the latest one or null if none found
        => principal.FindAll("exp")
            .Select(c => c.Value)
            .Select(long.Parse)
            .Select(DateTimeOffset.FromUnixTimeSeconds)
            .Max().DateTime is var exp && exp == DateTime.MinValue ? null : exp;

    /// <summary>
    /// Reads all manifests, validating their signatures.
    /// </summary>
    /// <param name="principal">The combined principal with all identities (and their claims) from each provided and valid JWT</param>
    /// <param name="values">The tokens to read and their corresponding JWK for signature verification.</param>
    /// <returns><see langword="true"/> if at least one manifest can be successfully read and is valid. 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TryRead([NotNullWhen(true)] out ClaimsPrincipal? principal, params (string jwt, string jwk)[] values)
        => TryRead(out principal, values.AsEnumerable());

    /// <summary>
    /// Reads all manifests, validating their signatures.
    /// </summary>
    /// <param name="principal">The combined principal with all identities (and their claims) from each provided and valid JWT</param>
    /// <param name="values">The tokens to read and their corresponding JWK for signature verification.</param>
    /// <returns><see langword="true"/> if at least one manifest can be successfully read and is valid. 
    /// <see langword="false"/> otherwise.</returns>
    public static bool TryRead([NotNullWhen(true)] out ClaimsPrincipal? principal, IEnumerable<(string jwt, string jwk)> values)
    {
        principal = null;

        foreach (var value in values)
        {
            if (string.IsNullOrWhiteSpace(value.jwt) || string.IsNullOrEmpty(value.jwk))
                continue;

            if (Validate(value.jwt, value.jwk, out var token, out var claims, false) == ManifestStatus.Valid && claims != null)
            {
                if (principal == null)
                    principal = claims;
                else
                    principal.AddIdentities(claims.Identities);
            }
        }

        return principal != null;
    }

    /// <summary>
    /// Validates the manifest signature and optional expiration.
    /// </summary>
    /// <param name="jwt">The JWT to validate.</param>
    /// <param name="jwk">The key to validate the manifest signature with.</param>
    /// <param name="token">Except when returning <see cref="Status.Unknown"/>, returns the security token read from the JWT, even if signature check failed.</param>
    /// <param name="principal">The associated claims, only when return value is not <see cref="Status.Unknown"/>.</param>
    /// <param name="requireExpiration">Whether to check for expiration.</param>
    /// <returns>The status of the validation.</returns>
    public static ManifestStatus Validate(string jwt, string jwk, out SecurityToken? token, out ClaimsPrincipal? principal, bool validateExpiration)
    {
        token = default;
        principal = default;

        SecurityKey key;
        try
        {
            key = JsonWebKey.Create(jwk);
        }
        catch (ArgumentException)
        {
            return ManifestStatus.Unknown;
        }

        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

        if (!handler.CanReadToken(jwt))
            return ManifestStatus.Unknown;

        var validation = new TokenValidationParameters
        {
            RequireExpirationTime = false,
            ValidateLifetime = false,
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            RoleClaimType = "roles",
            NameClaimType = "sub",
        };

        try
        {
            principal = handler.ValidateToken(jwt, validation, out token);
            if (validateExpiration && token.ValidTo == DateTime.MinValue)
                return ManifestStatus.Invalid;

            // The sponsorable manifest does not have an expiration time.
            if (validateExpiration && token.ValidTo < DateTimeOffset.UtcNow)
                return ManifestStatus.Expired;

            return ManifestStatus.Valid;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            var jwtToken = handler.ReadJwtToken(jwt);
            token = jwtToken;
            principal = new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims));
            return ManifestStatus.Invalid;
        }
        catch (SecurityTokenException)
        {
            var jwtToken = handler.ReadJwtToken(jwt);
            token = jwtToken;
            principal = new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims));
            return ManifestStatus.Invalid;
        }
    }

}
