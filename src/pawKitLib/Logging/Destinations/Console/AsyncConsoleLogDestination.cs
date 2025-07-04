using Microsoft.Extensions.Logging;
using PawKitLib.Logging.Core;
using PawKitLib.Logging.Destinations.Base;

namespace PawKitLib.Logging.Destinations.Console;

/// <summary>
/// An asynchronous log destination that writes log entries to the console.
/// </summary>
public sealed class AsyncConsoleLogDestination : BaseAsyncLogDestination
{
    private readonly bool _useColors;

    /// <summary>
    /// Initializes a new instance of the AsyncConsoleLogDestination class.
    /// </summary>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="useColors">Whether to use colors for different log levels.</param>
    public AsyncConsoleLogDestination(LogWriteMode writeMode, LogThreadSafety threadSafety, bool useColors = true)
        : base(writeMode, threadSafety)
    {
        _useColors = useColors;
    }

    /// <summary>
    /// Asynchronously writes a single log entry to the console.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    protected override Task WriteLogEntryAsync(LogEntry logEntry, CancellationToken cancellationToken)
    {
        var message = FormatLogEntry(logEntry);

        // Console operations are fast and don't benefit from Task.Run
        // Just write directly and return completed task
        if (_useColors)
        {
            var originalColor = System.Console.ForegroundColor;
            try
            {
                System.Console.ForegroundColor = GetLogLevelColor(logEntry.LogLevel);
                System.Console.WriteLine(message);
            }
            finally
            {
                System.Console.ForegroundColor = originalColor;
            }
        }
        else
        {
            System.Console.WriteLine(message);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Formats a log entry into a string representation.
    /// </summary>
    /// <param name="logEntry">The log entry to format.</param>
    /// <returns>The formatted log entry string.</returns>
    private static string FormatLogEntry(LogEntry logEntry)
    {
        var timestamp = logEntry.TimestampUtc.ToString("O"); // ISO 8601 roundtrip format
        var level = GetLogLevelString(logEntry.LogLevel);
        var category = logEntry.CategoryName;
        var message = logEntry.Message;

        var formattedMessage = $"[{timestamp}] [{level}] {category}: {message}";

        if (logEntry.Exception != null)
        {
            formattedMessage += Environment.NewLine + logEntry.Exception.ToString();
        }

        return formattedMessage;
    }

    /// <summary>
    /// Gets the string representation of a log level.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>The string representation of the log level.</returns>
    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "FAIL",
            LogLevel.Critical => "CRIT",
            LogLevel.None => "NONE",
            _ => logLevel.ToString().ToUpperInvariant()
        };
    }

    /// <summary>
    /// Gets the console color for a log level.
    /// </summary>
    /// <param name="logLevel">The log level.</param>
    /// <returns>The console color for the log level.</returns>
    private static ConsoleColor GetLogLevelColor(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => ConsoleColor.Gray,
            LogLevel.Debug => ConsoleColor.Gray,
            LogLevel.Information => ConsoleColor.White,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Critical => ConsoleColor.DarkRed,
            LogLevel.None => ConsoleColor.White,
            _ => ConsoleColor.White
        };
    }
}