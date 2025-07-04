using Microsoft.Extensions.Logging;

namespace PawKitLib.Logging;

/// <summary>
/// Provides centralized logging functionality for pawKit library.
/// This class serves as a bridge between the old static logging approach and the new PawKit logging system.
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
    /// Configures the logger factory using PawKit logging configuration.
    /// </summary>
    /// <param name="configureLogging">A delegate to configure the logging destinations.</param>
    public static void Configure(Action<LoggerConfiguration> configureLogging)
    {
        if (configureLogging == null)
            throw new ArgumentNullException(nameof(configureLogging));

        var configuration = LoggerConfiguration.Create();
        configureLogging(configuration);
        _loggerFactory = configuration.Build();
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

    /// <summary>
    /// Flushes all loggers if the configured factory supports it.
    /// </summary>
    public static void Flush()
    {
        if (_loggerFactory is PawKitLoggerFactory pawKitFactory)
        {
            pawKitFactory.Flush();
        }
    }

    /// <summary>
    /// Disposes the logger factory if it implements IDisposable.
    /// </summary>
    public static void Shutdown()
    {
        if (_loggerFactory is IDisposable disposableFactory)
        {
            disposableFactory.Dispose();
        }
        _loggerFactory = null;
    }
}