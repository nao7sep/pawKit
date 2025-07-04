using Microsoft.Extensions.Logging;

namespace PawKitLib.Logging;

/// <summary>
/// A log destination that writes log entries to the console.
/// </summary>
public sealed class ConsoleLogDestination : BaseLogDestination
{
    private readonly bool _useColors;

    /// <summary>
    /// Initializes a new instance of the ConsoleLogDestination class.
    /// </summary>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="useColors">Whether to use colors for different log levels.</param>
    public ConsoleLogDestination(WriteMode writeMode, ThreadSafety threadSafety, bool useColors = true)
        : base(writeMode, threadSafety)
    {
        _useColors = useColors;
    }

    /// <summary>
    /// Writes a single log entry to the console.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    protected override void WriteLogEntry(LogEntry logEntry)
    {
        var message = FormatLogEntry(logEntry);

        if (_useColors)
        {
            var originalColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = GetLogLevelColor(logEntry.LogLevel);
                Console.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = originalColor;
            }
        }
        else
        {
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Formats a log entry into a string representation.
    /// </summary>
    /// <param name="logEntry">The log entry to format.</param>
    /// <returns>The formatted log entry string.</returns>
    private static string FormatLogEntry(LogEntry logEntry)
    {
        var timestamp = logEntry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
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