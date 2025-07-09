using pawKitLib.Abstractions;

namespace pawKitLib.Core;

/// <summary>
/// A default implementation of <see cref="IClock"/> that uses the system's UTC clock.
/// </summary>
public sealed class SystemClock : IClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}