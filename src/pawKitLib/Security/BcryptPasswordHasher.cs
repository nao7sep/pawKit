using pawKitLib.Abstractions;

namespace pawKitLib.Security;

/// <summary>
/// An implementation of <see cref="IPasswordHasher"/> that uses the BCrypt hashing algorithm.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    /// <inheritdoc />
    public string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    /// <inheritdoc />
    public bool VerifyPassword(string passwordHash, string providedPassword) =>
        BCrypt.Net.BCrypt.Verify(providedPassword, passwordHash);
}