using Microsoft.Extensions.Logging;

namespace PawKitLib.Logging;

/// <summary>
/// A logger implementation that writes to multiple configured destinations.
/// </summary>
public sealed class PawKitLogger : ILogger
{
    private readonly string _categoryName;
    private readonly IReadOnlyList<ILogDestination> _destinations;
    private readonly LogLevel _minimumLevel;

    /// <summary>
    /// Initializes a new instance of the PawKitLogger class.
    /// </summary>
    /// <param name="categoryName">The category name for this logger.</param>
    /// <param name="destinations">The list of destinations to write to.</param>
    /// <param name="minimumLevel">The minimum log level to process.</param>
    public PawKitLogger(string categoryName, IReadOnlyList<ILogDestination> destinations, LogLevel minimumLevel = LogLevel.Information)
    {
        _categoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        _destinations = destinations ?? throw new ArgumentNullException(nameof(destinations));
        _minimumLevel = minimumLevel;
    }

    /// <summary>
    /// Begins a logical operation scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state to begin scope for.</typeparam>
    /// <param name="state">The identifier for the scope.</param>
    /// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        // Simple scope implementation - could be enhanced with actual scope tracking
        return new LogScope();
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

        var logEntry = new LogEntry(
            timestamp: DateTime.UtcNow,
            logLevel: logLevel,
            categoryName: _categoryName,
            eventId: eventId,
            message: message ?? string.Empty,
            exception: exception
        );

        // Write to all destinations
        foreach (var destination in _destinations)
        {
            try
            {
                destination.WriteLog(logEntry);
            }
            catch (Exception ex)
            {
                // If a destination fails, we don't want to crash the application
                // We could potentially log this to a fallback destination, but for now we'll ignore it
                // In a production system, you might want to have a fallback mechanism
                Console.WriteLine($"Failed to write to log destination: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Flushes all destinations that support buffering.
    /// </summary>
    public void Flush()
    {
        foreach (var destination in _destinations)
        {
            try
            {
                destination.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to flush log destination: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Simple implementation of a log scope.
    /// </summary>
    private sealed class LogScope : IDisposable
    {
        public void Dispose()
        {
            // Simple scope - no actual implementation needed for basic functionality
        }
    }
}