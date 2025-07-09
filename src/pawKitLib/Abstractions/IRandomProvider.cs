namespace pawKitLib.Abstractions;

/// <summary>
/// Defines a contract for a provider of cryptographically secure random data,
/// serving as a testable and injectable equivalent to <see cref="System.Random"/>.
/// </summary>
/// <remarks>
/// <para>
/// This abstraction allows for dependency injection and testability of components
/// that rely on random data generation. The default implementation should use a
/// cryptographically secure random number generator.
/// </para>
/// <para>
/// The interface is intentionally kept minimal, providing only the fundamental primitives
/// for generating random data. Higher-level functionality, such as shuffling collections
/// or generating random strings, is provided via extension methods in the
/// <see cref="pawKitLib.Utils.RandomProviderExtensions"/> class. This design adheres to the
/// Open/Closed Principle, allowing for a rich feature set without modifying the core contract.
/// </para>
/// </remarks>
public interface IRandomProvider
{
    /// <summary>
    /// Fills a byte array with a cryptographically strong random sequence of values.
    /// </summary>
    /// <param name="byteCount">The number of random bytes to generate.</param>
    /// <returns>An array containing the random bytes.</returns>
    byte[] GetBytes(int byteCount);

    /// <summary>
    /// Fills the elements of a specified span of bytes with a cryptographically strong random sequence of values.
    /// </summary>
    /// <param name="buffer">The span to fill with random bytes.</param>
    void GetBytes(Span<byte> buffer);

    /// <summary>
    /// Generates a cryptographically secure random integer that is within a specified range.
    /// </summary>
    /// <param name="fromInclusive">The inclusive lower bound of the random number returned.</param>
    /// <param name="toExclusive">The exclusive upper bound of the random number returned. Must be greater than or equal to <paramref name="fromInclusive"/>.</param>
    /// <returns>A random integer that is greater than or equal to <paramref name="fromInclusive"/> and less than <paramref name="toExclusive"/>.</returns>
    int GetInt32(int fromInclusive, int toExclusive);

    /// <summary>
    /// Generates a non-negative cryptographically secure random integer that is less than the specified maximum.
    /// </summary>
    /// <param name="toExclusive">The exclusive upper bound of the random number returned. Must be greater than or equal to 0.</param>
    /// <returns>A random integer that is greater than or equal to 0 and less than <paramref name="toExclusive"/>.</returns>
    int GetInt32(int toExclusive);

    /// <summary>
    /// Generates a non-negative cryptographically secure random integer.
    /// </summary>
    /// <returns>A random integer that is greater than or equal to 0 and less than <see cref="int.MaxValue"/>.</returns>
    /// <remarks>This method behaves identically to <see cref="System.Random.Next()"/>, producing a value in the range [0, int.MaxValue - 1].</remarks>
    int GetInt32();

    /// <summary>
    /// Generates a cryptographically secure random 64-bit integer that is within a specified range.
    /// </summary>
    /// <param name="fromInclusive">The inclusive lower bound of the random number returned.</param>
    /// <param name="toExclusive">The exclusive upper bound of the random number returned. Must be greater than or equal to <paramref name="fromInclusive"/>.</param>
    /// <returns>A random 64-bit integer that is greater than or equal to <paramref name="fromInclusive"/> and less than <paramref name="toExclusive"/>.</returns>
    long GetInt64(long fromInclusive, long toExclusive);

    /// <summary>
    /// Generates a non-negative cryptographically secure random 64-bit integer that is less than the specified maximum.
    /// </summary>
    /// <param name="toExclusive">The exclusive upper bound of the random number returned. Must be greater than or equal to 0.</param>
    /// <returns>A random 64-bit integer that is greater than or equal to 0 and less than <paramref name="toExclusive"/>.</returns>
    long GetInt64(long toExclusive);

    /// <summary>
    /// Generates a non-negative cryptographically secure random 64-bit integer.
    /// </summary>
    /// <returns>A random 64-bit integer that is greater than or equal to 0 and less than <see cref="long.MaxValue"/>.</returns>
    /// <remarks>This method behaves identically to <see cref="System.Random.NextInt64()"/>, producing a value in the range [0, long.MaxValue - 1].</remarks>
    long GetInt64();

    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0 and less than 1.0.
    /// </summary>
    /// <returns>A double-precision floating point number.</returns>
    double GetDouble();

    /// <summary>
    /// Returns a random floating-point number that is greater than or equal to 0.0 and less than 1.0.
    /// </summary>
    /// <returns>A single-precision floating point number.</returns>
    float GetSingle();
}