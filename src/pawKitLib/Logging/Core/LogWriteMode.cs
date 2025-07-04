namespace PawKitLib.Logging.Core;

/// <summary>
/// Specifies how log entries are written to their destination.
/// </summary>
public enum LogWriteMode
{
    /// <summary>
    /// Log entries are written immediately to the destination.
    /// </summary>
    Immediate,

    /// <summary>
    /// Log entries are buffered and written when the buffer is flushed.
    /// </summary>
    Buffered
}