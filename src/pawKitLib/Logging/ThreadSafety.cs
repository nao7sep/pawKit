namespace PawKitLib.Logging;

/// <summary>
/// Specifies the thread safety behavior for log destinations.
/// </summary>
public enum ThreadSafety
{
    /// <summary>
    /// Operations are not thread-safe. Use this for better performance when thread safety is not required.
    /// </summary>
    NotThreadSafe,

    /// <summary>
    /// Operations are thread-safe using internal locking mechanisms.
    /// </summary>
    ThreadSafe
}