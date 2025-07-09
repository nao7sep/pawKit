namespace pawKitLib.Abstractions;

/// <summary>
/// Defines a contract for generating cryptographically secure random strings.
/// </summary>
/// <remarks>
/// Suitable for creating API keys, refresh tokens, or other sensitive secrets.
/// </remarks>
public interface ISecretGenerator
{
    /// <summary>
    /// Generates a secure, random, URL-safe string of a specified length.
    /// </summary>
    /// <param name="length">The desired length of the secret string.</param>
    /// <returns>A cryptographically secure random string.</returns>
    string Generate(int length);
}