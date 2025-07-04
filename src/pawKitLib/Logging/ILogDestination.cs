namespace PawKitLib.Logging;

/// <summary>
/// Defines the contract for log destinations that can receive and process log entries.
/// </summary>
public interface ILogDestination : IDisposable
{
    /// <summary>
    /// Gets the write mode for this destination.
    /// </summary>
    LogWriteMode WriteMode { get; }

    /// <summary>
    /// Gets the thread safety mode for this destination.
    /// </summary>
    LogThreadSafety ThreadSafety { get; }

    /// <summary>
    /// Writes a log entry to the destination.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    void WriteLog(LogEntry logEntry);

    /// <summary>
    /// Flushes any buffered log entries to the destination.
    /// This method has no effect for destinations with WriteMode.Immediate.
    /// </summary>
    void Flush();
}