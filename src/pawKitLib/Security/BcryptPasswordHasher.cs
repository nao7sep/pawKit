using pawKitLib.Abstractions;

namespace pawKitLib.Security;

/// <summary>
/// An implementation of <see cref="IPasswordHasher"/> that uses the BCrypt hashing algorithm.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher
{
    // The bcrypt work factor (cost). This controls the computational complexity of the hash.
    // Higher values increase security by making brute-force attacks more expensive, but also slow down hashing.
    // The default value of 12 is widely recommended as a balance between security and performance for most modern systems.
    // With a work factor of 12, hashing a single password typically takes about 200–400 milliseconds on a modern consumer CPU.
    // This intentional delay makes large-scale brute-force attacks impractical, while remaining fast enough for normal user authentication.
    // Bcrypt will automatically generate a unique salt for each password and embed it in the resulting hash.
    // When verifying, bcrypt extracts the salt and work factor from the hash string.
    private readonly int _workFactor;

    /// <summary>
    /// Initializes a new instance of the <see cref="BcryptPasswordHasher"/> class with the specified work factor.
    /// </summary>
    /// <param name="workFactor">The bcrypt work factor (cost). Defaults to 12.</param>
    public BcryptPasswordHasher(int workFactor = 12)
    {
        _workFactor = workFactor;
    }

    /// <inheritdoc />
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password, _workFactor);

    /// <inheritdoc />
    public bool Verify(string passwordHash, string providedPassword) =>
        BCrypt.Net.BCrypt.Verify(providedPassword, passwordHash);
}