﻿namespace pawKitLib.Abstractions;

/// <summary>
/// Defines a contract for hashing and verifying passwords.
/// </summary>
/// <remarks>
/// This interface is specifically for handling user-chosen passwords. Its responsibility is to
/// securely hash a password for storage and verify a provided password against that hash.
/// For generating machine-generated secrets like API keys or tokens, use <see cref="IRandomProvider"/>.
/// </remarks>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>A string representing the hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies that a plain-text password matches a stored hash.
    /// </summary>
    /// <param name="passwordHash">The stored password hash.</param>
    /// <param name="providedPassword">The plain-text password provided by the user.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise, <c>false</c>.</returns>
    bool VerifyPassword(string passwordHash, string providedPassword);
}