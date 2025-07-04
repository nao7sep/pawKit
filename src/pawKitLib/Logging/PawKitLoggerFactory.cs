using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace PawKitLib.Logging;

/// <summary>
/// A logger factory implementation that creates PawKit loggers with configured destinations.
/// </summary>
public sealed class PawKitLoggerFactory : ILoggerFactory
{
    private readonly ConcurrentDictionary<string, PawKitLogger> _loggers = new();
    private readonly IReadOnlyList<ILogDestination> _destinations;
    private readonly LogLevel _minimumLevel;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the PawKitLoggerFactory class.
    /// </summary>
    /// <param name="destinations">The list of destinations to write to.</param>
    /// <param name="minimumLevel">The minimum log level to process.</param>
    public PawKitLoggerFactory(IReadOnlyList<ILogDestination> destinations, LogLevel minimumLevel = LogLevel.Information)
    {
        _destinations = destinations ?? throw new ArgumentNullException(nameof(destinations));
        _minimumLevel = minimumLevel;
    }

    /// <summary>
    /// Creates a new ILogger instance.
    /// </summary>
    /// <param name="categoryName">The category name for messages produced by the logger.</param>
    /// <returns>A new ILogger instance.</returns>
    public ILogger CreateLogger(string categoryName)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PawKitLoggerFactory));

        return _loggers.GetOrAdd(categoryName, name => new PawKitLogger(name, _destinations, _minimumLevel));
    }

    /// <summary>
    /// Adds an ILoggerProvider to the logging system.
    /// </summary>
    /// <param name="provider">The ILoggerProvider to add.</param>
    /// <remarks>This implementation does not support external providers.</remarks>
    public void AddProvider(ILoggerProvider provider)
    {
        // PawKit logger factory doesn't support external providers
        // This is intentional as we want to keep the system simple and self-contained
    }

    /// <summary>
    /// Flushes all loggers and their destinations.
    /// </summary>
    public void Flush()
    {
        if (_disposed)
            return;

        foreach (var logger in _loggers.Values)
        {
            logger.Flush();
        }
    }

    /// <summary>
    /// Releases all resources used by the PawKitLoggerFactory.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            // Flush all loggers before disposing
            Flush();

            // Dispose all destinations
            foreach (var destination in _destinations)
            {
                try
                {
                    destination.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error disposing log destination: {ex.Message}");
                }
            }

            _loggers.Clear();
        }
        finally
        {
            _disposed = true;
        }
    }
}