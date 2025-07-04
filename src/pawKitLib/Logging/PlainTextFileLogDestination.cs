using Microsoft.Extensions.Logging;

namespace PawKitLib.Logging;

/// <summary>
/// A log destination that writes log entries to a plain text file.
/// </summary>
public sealed class PlainTextFileLogDestination : BaseLogDestination
{
    private readonly string _filePath;
    private readonly bool _appendToFile;

    /// <summary>
    /// Initializes a new instance of the PlainTextFileLogDestination class.
    /// </summary>
    /// <param name="filePath">The path to the log file.</param>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="appendToFile">Whether to append to an existing file or overwrite it.</param>
    public PlainTextFileLogDestination(string filePath, WriteMode writeMode, ThreadSafety threadSafety, bool appendToFile = true)
        : base(writeMode, threadSafety)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _appendToFile = appendToFile;

        // Ensure the directory exists
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // If not appending, clear the file
        if (!_appendToFile && File.Exists(_filePath))
        {
            File.WriteAllText(_filePath, string.Empty);
        }
    }

    /// <summary>
    /// Writes a single log entry to the plain text file.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    protected override void WriteLogEntry(LogEntry logEntry)
    {
        var message = FormatLogEntry(logEntry);

        try
        {
            File.AppendAllText(_filePath, message + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // If we can't write to the file, write to console as fallback
            Console.WriteLine($"Failed to write to log file '{_filePath}': {ex.Message}");
            Console.WriteLine($"Log entry: {message}");
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
}