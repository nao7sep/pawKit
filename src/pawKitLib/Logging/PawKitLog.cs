using Microsoft.Extensions.Logging;

namespace PawKitLib.Logging;

/// <summary>
/// Provides centralized logging functionality for pawKit library.
/// </summary>
public static class PawKitLog
{
    private static ILoggerFactory? _loggerFactory;

    /// <summary>
    /// Configures the logger factory for the pawKit library.
    /// </summary>
    /// <param name="loggerFactory">The logger factory to use for creating loggers.</param>
    public static void Configure(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// Creates a logger for the specified type.
    /// </summary>
    /// <typeparam name="T">The type to create a logger for.</typeparam>
    /// <returns>An ILogger instance for the specified type.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the logger factory has not been configured.</exception>
    public static ILogger<T> CreateLogger<T>()
    {
        if (_loggerFactory == null)
            throw new InvalidOperationException("Logger factory has not been configured. Call Configure() first.");

        return _loggerFactory.CreateLogger<T>();
    }

    /// <summary>
    /// Creates a logger with the specified category name.
    /// </summary>
    /// <param name="categoryName">The category name for the logger.</param>
    /// <returns>An ILogger instance with the specified category name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the logger factory has not been configured.</exception>
    public static ILogger CreateLogger(string categoryName)
    {
        if (_loggerFactory == null)
            throw new InvalidOperationException("Logger factory has not been configured. Call Configure() first.");

        return _loggerFactory.CreateLogger(categoryName);
    }
}