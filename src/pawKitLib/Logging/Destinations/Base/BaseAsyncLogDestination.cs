using System.Collections.Concurrent;
using PawKitLib.Logging.Core;

namespace PawKitLib.Logging.Destinations.Base;

/// <summary>
/// Base class for asynchronous log destinations that provides common functionality for buffering and thread safety.
/// </summary>
public abstract class BaseAsyncLogDestination : IAsyncLogDestination
{
    private readonly ConcurrentQueue<LogEntry> _buffer;
    private readonly SemaphoreSlim? _semaphore;
    private readonly SemaphoreSlim _flushSemaphore = new(1, 1);
    private volatile int _bufferCount;
    private bool _disposed;

    /// <summary>
    /// Gets the write mode for this destination.
    /// </summary>
    public LogWriteMode WriteMode { get; }

    /// <summary>
    /// Gets the thread safety mode for this destination.
    /// </summary>
    public LogThreadSafety ThreadSafety { get; }

    /// <summary>
    /// Gets the maximum number of entries to buffer before automatic flush.
    /// Default is 100 entries.
    /// </summary>
    protected virtual int BufferSize => 100;

    /// <summary>
    /// Initializes a new instance of the BaseAsyncLogDestination class.
    /// </summary>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    protected BaseAsyncLogDestination(LogWriteMode writeMode, LogThreadSafety threadSafety)
    {
        WriteMode = writeMode;
        ThreadSafety = threadSafety;

        if (writeMode == LogWriteMode.Buffered)
        {
            _buffer = new ConcurrentQueue<LogEntry>();
        }
        else
        {
            _buffer = null!;
        }

        if (threadSafety == LogThreadSafety.ThreadSafe)
        {
            _semaphore = new SemaphoreSlim(1, 1);
        }
    }

    /// <summary>
    /// Asynchronously writes a log entry to the destination.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    public async Task WriteLogAsync(LogEntry logEntry, CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BaseAsyncLogDestination));

        if (ThreadSafety == LogThreadSafety.ThreadSafe)
        {
            await _semaphore!.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await WriteLogInternalAsync(logEntry, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        else
        {
            await WriteLogInternalAsync(logEntry, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously flushes any buffered log entries to the destination.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous flush operation.</returns>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BaseAsyncLogDestination));

        if (WriteMode == LogWriteMode.Immediate)
            return;

        // Use flush semaphore to prevent multiple concurrent flushes
        await _flushSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (ThreadSafety == LogThreadSafety.ThreadSafe)
            {
                await _semaphore!.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    await FlushInternalAsync(cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    _semaphore.Release();
                }
            }
            else
            {
                await FlushInternalAsync(cancellationToken).ConfigureAwait(false);
            }
        }
        finally
        {
            _flushSemaphore.Release();
        }
    }

    /// <summary>
    /// Internal method to handle asynchronous log writing logic.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    private async Task WriteLogInternalAsync(LogEntry logEntry, CancellationToken cancellationToken)
    {
        if (WriteMode == LogWriteMode.Immediate)
        {
            await WriteLogEntryAsync(logEntry, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            _buffer.Enqueue(logEntry);
            var currentCount = Interlocked.Increment(ref _bufferCount);
            if (currentCount >= BufferSize)
            {
                await FlushInternalAsync(cancellationToken).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Internal method to handle asynchronous flushing logic.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous flush operation.</returns>
    private async Task FlushInternalAsync(CancellationToken cancellationToken)
    {
        if (_bufferCount == 0)
            return;

        var entriesToWrite = new List<LogEntry>();

        // Dequeue all current entries
        while (_buffer.TryDequeue(out var entry))
        {
            entriesToWrite.Add(entry);
            Interlocked.Decrement(ref _bufferCount);
        }

        // Write all entries
        foreach (var entry in entriesToWrite)
        {
            await WriteLogEntryAsync(entry, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// When overridden in a derived class, asynchronously writes a single log entry to the specific destination.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    protected abstract Task WriteLogEntryAsync(LogEntry logEntry, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously releases all resources used by the BaseAsyncLogDestination.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        try
        {
            if (WriteMode == LogWriteMode.Buffered)
            {
                await FlushAsync().ConfigureAwait(false);
            }
        }
        catch
        {
            // Ignore exceptions during disposal
        }

        _semaphore?.Dispose();
        _flushSemaphore.Dispose();
        _disposed = true;
    }
}