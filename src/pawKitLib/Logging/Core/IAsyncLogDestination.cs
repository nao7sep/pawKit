namespace PawKitLib.Logging.Core;

/// <summary>
/// Defines the contract for asynchronous log destinations that can receive and process log entries.
/// </summary>
public interface IAsyncLogDestination : IAsyncDisposable
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
    /// Asynchronously writes a log entry to the destination.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    Task WriteLogAsync(LogEntry logEntry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously flushes any buffered log entries to the destination.
    /// This method has no effect for destinations with WriteMode.Immediate.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous flush operation.</returns>
    Task FlushAsync(CancellationToken cancellationToken = default);
}