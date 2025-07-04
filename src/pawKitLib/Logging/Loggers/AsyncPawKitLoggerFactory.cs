using Microsoft.Extensions.Logging;
using PawKitLib.Logging.Core;
using System.Collections.Concurrent;

namespace PawKitLib.Logging.Loggers;

/// <summary>
/// An asynchronous logger factory implementation that creates PawKit loggers with configured async destinations.
/// </summary>
public sealed class AsyncPawKitLoggerFactory : ILoggerFactory, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, AsyncPawKitLogger> _loggers = new();
    private readonly IReadOnlyList<IAsyncLogDestination> _destinations;
    private readonly LogLevel _minimumLevel;
    private readonly int _channelCapacity;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the AsyncPawKitLoggerFactory class.
    /// </summary>
    /// <param name="destinations">The list of async destinations to write to.</param>
    /// <param name="minimumLevel">The minimum log level to process.</param>
    /// <param name="channelCapacity">The capacity of the internal channel for buffering log entries. Default is 1000.</param>
    public AsyncPawKitLoggerFactory(IReadOnlyList<IAsyncLogDestination> destinations, LogLevel minimumLevel = LogLevel.Information, int channelCapacity = 1000)
    {
        _destinations = destinations ?? throw new ArgumentNullException(nameof(destinations));
        _minimumLevel = minimumLevel;
        _channelCapacity = channelCapacity > 0 ? channelCapacity : throw new ArgumentOutOfRangeException(nameof(channelCapacity), "Channel capacity must be greater than zero.");
    }

    /// <summary>
    /// Creates a new ILogger instance.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>A new ILogger instance.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AsyncPawKitLoggerFactory));

        return _loggers.GetOrAdd(categoryName, name => new AsyncPawKitLogger(name, _destinations, _minimumLevel, _channelCapacity));
    }

    /// <summary>
    /// Adds an ILoggerProvider to the logging system.
    /// </summary>
    /// <param name="provider">The ILoggerProvider to add.</param>
    /// <remarks>This implementation does not support external providers.</remarks>
    public void AddProvider(ILoggerProvider provider)
    {
        // PawKit async logger factory doesn't support external providers
        // This is intentional as we want to keep the system simple and self-contained
    }

    /// <summary>
    /// Asynchronously flushes all loggers and their destinations.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous flush operation.</returns>
    public async Task FlushAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed)
            return;

        var tasks = _loggers.Values.Select(logger => logger.FlushAsync(cancellationToken));
        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    /// <summary>
    /// Synchronously flushes all loggers and their destinations.
    /// </summary>
    public void Flush()
    {
        // Use Task.Run to avoid potential deadlocks in sync-over-async
        Task.Run(async () => await FlushAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Releases all resources used by the AsyncPawKitLoggerFactory.
    /// </summary>
    public void Dispose()
    {
        // Use Task.Run to avoid potential deadlocks in sync-over-async
        Task.Run(async () => await DisposeAsync().ConfigureAwait(false)).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Asynchronously releases all resources used by the AsyncPawKitLoggerFactory.
    /// </summary>
    /// <returns>A task representing the asynchronous disposal operation.</returns>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;

        try
        {
            // Dispose all loggers first (they implement IAsyncDisposable now)
            var loggerDisposeTasks = _loggers.Values.Select(async logger =>
            {
                try
                {
                    await logger.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Error disposing async logger: {ex.Message}").ConfigureAwait(false);
                }
            });

            await Task.WhenAll(loggerDisposeTasks).ConfigureAwait(false);

            // Dispose all destinations
            var destinationDisposeTasks = _destinations.Select(async destination =>
            {
                try
                {
                    await destination.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync($"Error disposing async log destination: {ex.Message}").ConfigureAwait(false);
                }
            });

            await Task.WhenAll(destinationDisposeTasks).ConfigureAwait(false);

            _loggers.Clear();
        }
        finally
        {
            _disposed = true;
        }
    }
}