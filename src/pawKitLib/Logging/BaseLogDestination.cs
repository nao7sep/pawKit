using System.Collections.Concurrent;

namespace PawKitLib.Logging;

/// <summary>
/// Base class for log destinations that provides common functionality for buffering and thread safety.
/// </summary>
public abstract class BaseLogDestination : ILogDestination
{
    private readonly List<LogEntry> _buffer;
    private readonly ReaderWriterLockSlim? _lock;
    private readonly object _flushLock = new();
    private bool _disposed;

    /// <summary>
    /// Gets the write mode for this destination.
    /// </summary>
    public WriteMode WriteMode { get; }

    /// <summary>
    /// Gets the thread safety mode for this destination.
    /// </summary>
    public ThreadSafety ThreadSafety { get; }

    /// <summary>
    /// Gets the maximum number of entries to buffer before automatic flush.
    /// Default is 100 entries.
    /// </summary>
    protected virtual int BufferSize => 100;

    /// <summary>
    /// Initializes a new instance of the BaseLogDestination class.
    /// </summary>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    protected BaseLogDestination(WriteMode writeMode, ThreadSafety threadSafety)
    {
        WriteMode = writeMode;
        ThreadSafety = threadSafety;

        if (writeMode == WriteMode.Buffered)
        {
            _buffer = new List<LogEntry>();
        }
        else
        {
            _buffer = null!;
        }

        if (threadSafety == ThreadSafety.ThreadSafe)
        {
            _lock = new ReaderWriterLockSlim();
        }
    }

    /// <summary>
    /// Writes a log entry to the destination.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    public void WriteLog(LogEntry logEntry)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BaseLogDestination));

        if (ThreadSafety == ThreadSafety.ThreadSafe)
        {
            _lock!.EnterWriteLock();
            try
            {
                WriteLogInternal(logEntry);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        else
        {
            WriteLogInternal(logEntry);
        }
    }

    /// <summary>
    /// Flushes any buffered log entries to the destination.
    /// </summary>
    public void Flush()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(BaseLogDestination));

        if (WriteMode == WriteMode.Immediate)
            return;

        lock (_flushLock)
        {
            if (ThreadSafety == ThreadSafety.ThreadSafe)
            {
                _lock!.EnterWriteLock();
                try
                {
                    FlushInternal();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            else
            {
                FlushInternal();
            }
        }
    }

    /// <summary>
    /// Internal method to handle log writing logic.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    private void WriteLogInternal(LogEntry logEntry)
    {
        if (WriteMode == WriteMode.Immediate)
        {
            WriteLogEntry(logEntry);
        }
        else
        {
            _buffer.Add(logEntry);
            if (_buffer.Count >= BufferSize)
            {
                FlushInternal();
            }
        }
    }

    /// <summary>
    /// Internal method to handle flushing logic.
    /// </summary>
    private void FlushInternal()
    {
        if (_buffer.Count == 0)
            return;

        var entriesToWrite = _buffer.ToArray();
        _buffer.Clear();

        foreach (var entry in entriesToWrite)
        {
            WriteLogEntry(entry);
        }
    }

    /// <summary>
    /// When overridden in a derived class, writes a single log entry to the specific destination.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    protected abstract void WriteLogEntry(LogEntry logEntry);

    /// <summary>
    /// Releases all resources used by the BaseLogDestination.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            if (WriteMode == WriteMode.Buffered)
            {
                Flush();
            }
        }
        catch
        {
            // Ignore exceptions during disposal
        }

        _lock?.Dispose();
        _disposed = true;
    }
}