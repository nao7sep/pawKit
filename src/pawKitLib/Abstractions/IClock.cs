namespace pawKitLib.Abstractions;

/// <summary>
/// Provides an abstraction for the system clock to enable testability.
/// </summary>
public interface IClock
{
    /// <summary>
    /// Gets the current Coordinated Universal Time (UTC).
    /// </summary>
    DateTimeOffset UtcNow { get; }
}