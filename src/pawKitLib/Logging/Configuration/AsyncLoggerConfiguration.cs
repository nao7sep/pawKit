using Microsoft.Extensions.Logging;
using PawKitLib.Logging.Core;
using PawKitLib.Logging.Loggers;
using PawKitLib.Logging.Destinations.Console;
using PawKitLib.Logging.Destinations.File;
using PawKitLib.Logging.Destinations.Database;

namespace PawKitLib.Logging.Configuration;

/// <summary>
/// Provides a fluent API for configuring PawKit asynchronous logging destinations.
/// </summary>
public sealed class AsyncLoggerConfiguration
{
    private readonly List<IAsyncLogDestination> _destinations = new();
    private LogLevel _minimumLevel = LogLevel.Information;
    private int _channelCapacity = 1000;

    /// <summary>
    /// Sets the minimum log level that will be processed.
    /// </summary>
    /// <param name="minimumLevel">The minimum log level.</param>
    /// <returns>The current AsyncLoggerConfiguration instance for method chaining.</returns>
    public AsyncLoggerConfiguration SetMinimumLevel(LogLevel minimumLevel)
    {
        _minimumLevel = minimumLevel;
        return this;
    }

    /// <summary>
    /// Adds an asynchronous console output destination.
    /// </summary>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="useColors">Whether to use colors for different log levels.</param>
    /// <returns>The current AsyncLoggerConfiguration instance for method chaining.</returns>
    public AsyncLoggerConfiguration AddAsyncConsole(LogWriteMode writeMode = LogWriteMode.Immediate, LogThreadSafety threadSafety = LogThreadSafety.ThreadSafe, bool useColors = true)
    {
        var destination = new AsyncConsoleLogDestination(writeMode, threadSafety, useColors);
        _destinations.Add(destination);
        return this;
    }

    /// <summary>
    /// Sets the channel capacity for async loggers.
    /// </summary>
    /// <param name="capacity">The capacity of the internal channel for buffering log entries.</param>
    /// <returns>The current AsyncLoggerConfiguration instance for method chaining.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is less than or equal to zero.</exception>
    public AsyncLoggerConfiguration SetChannelCapacity(int capacity)
    {
        _channelCapacity = capacity > 0 ? capacity : throw new ArgumentOutOfRangeException(nameof(capacity), "Channel capacity must be greater than zero.");
        return this;
    }

    /// <summary>
    /// Adds an asynchronous plain text file output destination.
    /// </summary>
    /// <param name="filePath">The path to the log file.</param>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="appendToFile">Whether to append to an existing file or overwrite it.</param>
    /// <returns>The current AsyncLoggerConfiguration instance for method chaining.</returns>
    public AsyncLoggerConfiguration AddAsyncPlainText(string filePath, LogWriteMode writeMode = LogWriteMode.Buffered, LogThreadSafety threadSafety = LogThreadSafety.ThreadSafe, bool appendToFile = true)
    {
        ValidateFilePath(filePath, nameof(filePath));

        var destination = new AsyncPlainTextFileLogDestination(filePath, writeMode, threadSafety, appendToFile);
        _destinations.Add(destination);
        return this;
    }

    /// <summary>
    /// Adds an asynchronous JSON file output destination.
    /// </summary>
    /// <param name="filePath">The path to the log file.</param>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="appendToFile">Whether to append to an existing file or overwrite it.</param>
    /// <returns>The current AsyncLoggerConfiguration instance for method chaining.</returns>
    public AsyncLoggerConfiguration AddAsyncJson(string filePath, LogWriteMode writeMode = LogWriteMode.Buffered, LogThreadSafety threadSafety = LogThreadSafety.ThreadSafe, bool appendToFile = true)
    {
        ValidateFilePath(filePath, nameof(filePath));

        var destination = new AsyncJsonFileLogDestination(filePath, writeMode, threadSafety, appendToFile);
        _destinations.Add(destination);
        return this;
    }

    /// <summary>
    /// Adds an asynchronous SQLite database output destination.
    /// </summary>
    /// <param name="filePath">The path to the SQLite database file.</param>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="createIfNotExists">Whether to create the database and table if they don't exist.</param>
    /// <returns>The current AsyncLoggerConfiguration instance for method chaining.</returns>
    public AsyncLoggerConfiguration AddAsyncSqlite(string filePath, LogWriteMode writeMode = LogWriteMode.Buffered, LogThreadSafety threadSafety = LogThreadSafety.ThreadSafe, bool createIfNotExists = true, int maxPoolSize = 10)
    {
        ValidateFilePath(filePath, nameof(filePath));

        var destination = new AsyncSqliteLogDestination(filePath, writeMode, threadSafety, createIfNotExists, maxPoolSize);
        _destinations.Add(destination);
        return this;
    }

    /// <summary>
    /// Adds a custom asynchronous log destination.
    /// </summary>
    /// <param name="destination">The custom async log destination to add.</param>
    /// <returns>The current AsyncLoggerConfiguration instance for method chaining.</returns>
    public AsyncLoggerConfiguration AddAsyncDestination(IAsyncLogDestination destination)
    {
        if (destination == null)
            throw new ArgumentNullException(nameof(destination));

        _destinations.Add(destination);
        return this;
    }

    /// <summary>
    /// Builds and returns a configured AsyncPawKitLoggerFactory.
    /// </summary>
    /// <returns>A configured AsyncPawKitLoggerFactory instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no destinations have been configured.</exception>
    public AsyncPawKitLoggerFactory Build()
    {
        if (_destinations.Count == 0)
            throw new InvalidOperationException("At least one async log destination must be configured.");

        return new AsyncPawKitLoggerFactory(_destinations.AsReadOnly(), _minimumLevel, _channelCapacity);
    }

    /// <summary>
    /// Creates a new AsyncLoggerConfiguration instance.
    /// </summary>
    /// <returns>A new AsyncLoggerConfiguration instance.</returns>
    public static AsyncLoggerConfiguration Create()
    {
        return new AsyncLoggerConfiguration();
    }

    /// <summary>
    /// Validates a file path parameter.
    /// </summary>
    /// <param name="filePath">The file path to validate.</param>
    /// <param name="parameterName">The parameter name for exception reporting.</param>
    /// <exception cref="ArgumentException">Thrown when the file path is invalid.</exception>
    private static void ValidateFilePath(string filePath, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null, empty, or whitespace.", parameterName);

        try
        {
            // Validate that the path is not just invalid characters
            var fullPath = Path.GetFullPath(filePath);
            var directory = Path.GetDirectoryName(fullPath);
            var fileName = Path.GetFileName(fullPath);

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File path must include a valid file name.", parameterName);

            // Check for invalid characters in the file name
            var invalidChars = Path.GetInvalidFileNameChars();
            if (fileName.IndexOfAny(invalidChars) >= 0)
                throw new ArgumentException($"File name contains invalid characters: {string.Join(", ", invalidChars.Where(c => fileName.Contains(c)))}.", parameterName);

            // Check for invalid characters in the directory path
            if (!string.IsNullOrEmpty(directory))
            {
                var invalidPathChars = Path.GetInvalidPathChars();
                if (directory.IndexOfAny(invalidPathChars) >= 0)
                    throw new ArgumentException($"Directory path contains invalid characters: {string.Join(", ", invalidPathChars.Where(c => directory.Contains(c)))}.", parameterName);
            }
        }
        catch (ArgumentException)
        {
            throw; // Re-throw our validation exceptions
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Invalid file path: {ex.Message}", parameterName, ex);
        }
    }
}