using System.Security.Cryptography;
using pawKitLib.Abstractions;

namespace pawKitLib.Security;

/// <summary>
/// A default implementation of <see cref="IRandomProvider"/> that uses the
/// built-in <see cref="RandomNumberGenerator"/> for cryptographic-quality randomness.
/// </summary>
public sealed class CryptoRandomProvider : IRandomProvider
{
    /// <inheritdoc />
    public byte[] GetBytes(int byteCount) =>
        RandomNumberGenerator.GetBytes(byteCount);

    /// <inheritdoc />
    public void GetBytes(Span<byte> buffer) =>
        RandomNumberGenerator.Fill(buffer);

    /// <inheritdoc />
    public int GetInt32(int fromInclusive, int toExclusive) =>
        RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);

    /// <inheritdoc />
    public int GetInt32(int toExclusive) =>
        RandomNumberGenerator.GetInt32(toExclusive);

    /// <inheritdoc />
    public int GetInt32() =>
        RandomNumberGenerator.GetInt32(int.MaxValue);

    /// <inheritdoc />
    public long GetInt64(long fromInclusive, long toExclusive) =>
        RandomNumberGenerator.GetInt64(fromInclusive, toExclusive);

    /// <inheritdoc />
    public long GetInt64(long toExclusive) =>
        RandomNumberGenerator.GetInt64(toExclusive);

    /// <inheritdoc />
    public long GetInt64() =>
        RandomNumberGenerator.GetInt64(long.MaxValue);

    /// <inheritdoc />
    public double GetDouble()
    {
        // To get a value in the range [0.0, 1.0), we generate a random long in the range
        // [0, long.MaxValue - 1] and then divide by long.MaxValue. This ensures the result
        // is always strictly less than 1.0.
        long randomLong = GetInt64(long.MaxValue);
        return (double)randomLong / long.MaxValue;
    }

    /// <inheritdoc />
    public float GetSingle()
    {
        // To get a value in the range [0.0f, 1.0f), we use a similar logic to GetDouble()
        // but with single-precision floating-point numbers in mind. We generate a random
        // integer in the range [0, int.MaxValue - 1] and divide by int.MaxValue.
        // This prevents rounding errors that could result in 1.0f if we were to cast from a double.
        int randomInt = GetInt32(int.MaxValue);
        return (float)randomInt / int.MaxValue;
    }
}