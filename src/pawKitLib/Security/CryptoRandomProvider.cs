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
    public long GetInt64(long fromInclusive, long toExclusive)
    {
        if (fromInclusive >= toExclusive)
        {
            throw new ArgumentException("fromInclusive must be less than toExclusive.", nameof(fromInclusive));
        }

        var range = (ulong)(toExclusive - fromInclusive);

        // If there's only one possible value, return it.
        if (range == 1)
        {
            return fromInclusive;
        }

        // To generate a cryptographically secure and statistically uniform random number within a
        // specific range, we use a technique called "rejection sampling".
        //
        // THE PROBLEM: MODULO BIAS
        // A naive approach, such as taking a random ulong and using the modulo operator
        // (`randomUlong % range`), would create a statistical bias. This is because the total
        // number of possible ulong values (2^64) is not an even multiple of most possible ranges.
        // The "leftover" values would make the first few numbers in the range slightly more
        // likely to be chosen.
        //
        // THE SOLUTION: REJECTION SAMPLING
        // We calculate the largest multiple of `range` that is less than or equal to ulong.MaxValue.
        // This defines a "safe" upper bound (`maxMultiple`). Any random number generated within this
        // safe zone is guaranteed to be part of a complete, unbiased set. If we generate a number
        // *outside* this safe zone, we simply "reject" it and try again.
        //
        // PERFORMANCE & COST
        // The `do-while` loop implements this rejection. While it looks like it could loop
        // indefinitely, the probability of rejection is extremely low for most ranges. In the
        // worst-case scenario (a range slightly more than half of ulong.MaxValue), the chance of
        // rejection is just under 50% per attempt. The probability of needing many attempts
        // decreases exponentially (e.g., 10 attempts is ~0.1%). For almost all practical ranges,
        // this loop will execute only once. The performance cost is negligible.
        var maxMultiple = ulong.MaxValue - (ulong.MaxValue % range + 1) % range;

        // Buffer for the random bytes. Declared here to avoid re-allocation in the loop.
        var buffer = new byte[sizeof(ulong)];
        ulong randomUlong;
        do
        {
            RandomNumberGenerator.Fill(buffer);
            randomUlong = BitConverter.ToUInt64(buffer);
        } while (randomUlong > maxMultiple);

        return (long)((randomUlong % range) + (ulong)fromInclusive);
    }

    /// <inheritdoc />
    public long GetInt64(long toExclusive) =>
        GetInt64(0, toExclusive);

    /// <inheritdoc />
    public long GetInt64() =>
        GetInt64(0, long.MaxValue);

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