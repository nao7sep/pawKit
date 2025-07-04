using Microsoft.Extensions.Logging;
using PawKitLib.Logging.Core;
using PawKitLib.Logging.Structured;
using System.Threading.Channels;

namespace PawKitLib.Logging.Loggers;

/// <summary>
/// An asynchronous logger implementation that writes to multiple configured destinations.
/// </summary>
public sealed class AsyncPawKitLogger : ILogger, IAsyncDisposable
{
    private readonly string _categoryName;
    private readonly IReadOnlyList<IAsyncLogDestination> _destinations;
    private readonly LogLevel _minimumLevel;
    private readonly Channel<LogEntry> _logChannel;
    private readonly ChannelWriter<LogEntry> _writer;
    private readonly Task _backgroundTask;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly int _channelCapacity;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the AsyncPawKitLogger class.
    /// </summary>
    /// <param name="categoryName">The category name for this logger.</param>
    /// <param name="destinations">The list of async destinations to write to.</param>
    /// <param name="minimumLevel">The minimum log level to process.</param>
    /// <param name="channelCapacity">The capacity of the internal channel for buffering log entries. Default is 1000.</param>
    public AsyncPawKitLogger(string categoryName, IReadOnlyList<IAsyncLogDestination> destinations, LogLevel minimumLevel = LogLevel.Information, int channelCapacity = 1000)
    {
        _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        _destinations = destinations ?? throw new ArgumentNullException(nameof(destinations));
        _minimumLevel = minimumLevel;
        _channelCapacity = channelCapacity > 0 ? channelCapacity : throw new ArgumentOutOfRangeException(nameof(channelCapacity), "Channel capacity must be greater than zero.");
        _cancellationTokenSource = new CancellationTokenSource();

        // Create bounded channel to prevent memory issues under high load
        var options = new BoundedChannelOptions(_channelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        };

        _logChannel = Channel.CreateBounded<LogEntry>(options);
        _writer = _logChannel.Writer;

        // Start background processing task
        _backgroundTask = ProcessLogEntriesAsync(_cancellationTokenSource.Token);
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return LogScope.BeginScope(state);
    }

    /// <summary>
    /// Checks if the given logLevel is enabled.
    /// </summary>
    /// <param name="logLevel">Level to be checked.</param>
    /// <returns>true if enabled; false otherwise.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None && logLevel >= _minimumLevel;
    }

    /// <summary>
    /// Writes a log entry.
    /// </summary>
    /// <typeparam name="TState">The type of the object to be written.</typeparam>
    /// <param name="logLevel">Entry will be written on this level.</param>
    /// <param name="eventId">Id of the event.</param>
    /// <param name="state">The entry to be written. Can be also an object.</param>
    /// <param name="exception">The exception related to this entry.</param>
    /// <param name="formatter">Function to create a string message of the state and exception.</param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        if (formatter == null)
            throw new ArgumentNullException(nameof(formatter));

        var message = formatter(state, exception);
        if (string.IsNullOrEmpty(message) && exception == null)
            return;

        // Extract structured properties and message template
        string? messageTemplate = null;
        IReadOnlyDictionary<string, object?>? properties = null;

        if (state is StructuredLogState structuredState)
        {
            messageTemplate = structuredState.MessageTemplate;
            properties = structuredState.Properties;
        }

        // Get scoped properties
        var scopeProperties = LogScope.GetAllScopeProperties();

        var logEntry = new LogEntry(
            timestampUtc: DateTime.UtcNow,
            logLevel: logLevel,
            categoryName: _categoryName,
            eventId: eventId,
            message: message ?? string.Empty,
            exception: exception,
            messageTemplate: messageTemplate,
            properties: properties,
            scopeProperties: scopeProperties
        );

        // Enqueue the log entry for background processing
        if (!_writer.TryWrite(logEntry))
        {
            // Channel is full - try to wait and write, or use fallback for critical logs
            try
            {
                // For critical logs, write directly as fallback to ensure they're not lost
                if (logLevel >= LogLevel.Error)
                {
                    _ = Task.Run(async () => await LogAsync(logEntry).ConfigureAwait(false));
                }
                else
                {
                    // For other logs, try to wait a short time and retry
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _writer.WriteAsync(logEntry, _cancellationTokenSource.Token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException)
                        {
                            // Logger is shutting down, drop the entry
                        }
                        catch
                        {
                            // If we still can't write, drop the entry to prevent blocking
                        }
                    });
                }
            }
            catch
            {
                // If all else fails, drop the entry to prevent application blocking
            }
        }
    }

    /// <summary>
    /// Processes log entries from the background channel.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for shutdown.</param>
    /// <returns>A task representing the background processing.</returns>
    private async Task ProcessLogEntriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var logEntry in _logChannel.Reader.ReadAllAsync(cancellationToken))
            {
                await LogAsync(logEntry).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        catch (Exception ex)
        {
            // Log processing error - try to write to console as last resort
            try
            {
                await Console.Out.WriteLineAsync($"Background log processing failed: {ex.Message}").ConfigureAwait(false);
            }
            catch
            {
                // If even console fails, there's nothing more we can do
            }
        }
    }

    /// <summary>
    /// Asynchronously writes a log entry to all destinations.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    /// <returns>A task representing the asynchronous logging operation.</returns>
    private async Task LogAsync(LogEntry logEntry)
    {
        // Skip flush marker entries
        if (logEntry.LogLevel == LogLevel.None && logEntry.CategoryName == "__FLUSH_MARKER__")
        {
            return;
        }

        // Write to all destinations concurrently
        var tasks = _destinations.Select(async destination =>
        {
            try
            {
                await destination.WriteLogAsync(logEntry).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // If a destination fails, we don't want to crash the application
                // Log to console as fallback, but don't block
                _ = Task.Run(() => Console.WriteLine($"Failed to write to async log destination: {ex.Message}"));
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously flushes all destinations that support buffering.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous flush operation.</returns>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        // Create a special flush marker entry to ensure all previous entries are processed
        var flushMarker = new LogEntry(
            timestampUtc: DateTime.UtcNow,
            logLevel: LogLevel.None, // Special marker level
            categoryName: "__FLUSH_MARKER__",
            eventId: new EventId(-1, "FlushMarker"),
            message: string.Empty,
            exception: null,
            messageTemplate: null,
            properties: null,
            scopeProperties: null
        );

        // Try to write the flush marker (use a non-canceled token for the marker)
        var markerWritten = false;
        try
        {
            // Use a short timeout instead of the potentially canceled token for the marker
            using var markerCts = new CancellationTokenSource(TimeSpan.FromMilliseconds(500));
            await _writer.WriteAsync(flushMarker, markerCts.Token).ConfigureAwait(false);
            markerWritten = true;
        }
        catch (OperationCanceledException)
        {
            // Logger is shutting down or marker timeout
        }
        catch
        {
            // Channel might be closed or full
        }

        if (markerWritten)
        {
            // Wait for the background task to process the marker (with timeout)
            var timeout = TimeSpan.FromMilliseconds(2000);
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            while (stopwatch.Elapsed < timeout && !cancellationToken.IsCancellationRequested)
            {
                // Check if the channel is empty (marker has been processed)
                if (_logChannel.Reader.Count == 0)
                    break;

                try
                {
                    await Task.Delay(10, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation requested, but continue to flush destinations
                    break;
                }
            }
        }
        else
        {
            // If we couldn't write the marker, wait a bit for any pending entries
            try
            {
                await Task.Delay(100, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Continue to flush destinations even if canceled
            }
        }

        // Now flush all destinations (don't pass the cancellation token to avoid failing the flush)
        var tasks = _destinations.Select(async destination =>
        {
            try
            {
                await destination.FlushAsync(CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"Failed to flush async log destination: {ex.Message}").ConfigureAwait(false);
            }
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously disposes the logger and ensures all logs are written.
    /// </summary>
    /// <returns>A task representing the disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        try
        {
            // Signal no more entries and wait for background processing to complete
            _writer.Complete();
            await _backgroundTask.ConfigureAwait(false);

            // Flush all destinations
            await FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            _cancellationTokenSource.Dispose();
        }
    }
}