using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace PawKitLib.Logging;

/// <summary>
/// A log destination that writes log entries to a JSON file.
/// </summary>
public sealed class JsonFileLogDestination : BaseLogDestination
{
    private readonly string _filePath;
    private readonly bool _appendToFile;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the JsonFileLogDestination class.
    /// </summary>
    /// <param name="filePath">The path to the log file.</param>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="appendToFile">Whether to append to an existing file or overwrite it.</param>
    public JsonFileLogDestination(string filePath, LogWriteMode writeMode, LogThreadSafety threadSafety, bool appendToFile = true)
        : base(writeMode, threadSafety)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _appendToFile = appendToFile;

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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
    /// Writes a single log entry to the JSON file.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    protected override void WriteLogEntry(LogEntry logEntry)
    {
        var jsonLogEntry = CreateJsonLogEntry(logEntry);

        try
        {
            var jsonString = JsonSerializer.Serialize(jsonLogEntry, _jsonOptions);
            File.AppendAllText(_filePath, jsonString + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // If we can't write to the file, write to console as fallback
            Console.WriteLine($"Failed to write to JSON log file '{_filePath}': {ex.Message}");
            Console.WriteLine($"Log entry: {logEntry.Message}");
        }
    }

    /// <summary>
    /// Creates a JSON-serializable object from a log entry.
    /// </summary>
    /// <param name="logEntry">The log entry to convert.</param>
    /// <returns>An object suitable for JSON serialization.</returns>
    private static object CreateJsonLogEntry(LogEntry logEntry)
    {
        var jsonEntry = new
        {
            timestampUtc = logEntry.TimestampUtc.ToString("O"), // ISO 8601 roundtrip format
            level = logEntry.LogLevel.ToString(),
            category = logEntry.CategoryName,
            eventId = new
            {
                id = logEntry.EventId.Id,
                name = logEntry.EventId.Name
            },
            message = logEntry.Message,
            exception = logEntry.Exception != null ? new
            {
                type = logEntry.Exception.GetType().Name,
                message = logEntry.Exception.Message,
                stackTrace = logEntry.Exception.StackTrace,
                innerException = GetInnerExceptionInfo(logEntry.Exception.InnerException)
            } : null
        };

        return jsonEntry;
    }

    /// <summary>
    /// Recursively gets inner exception information.
    /// </summary>
    /// <param name="exception">The inner exception.</param>
    /// <returns>Inner exception information or null.</returns>
    private static object? GetInnerExceptionInfo(Exception? exception)
    {
        if (exception == null)
            return null;

        return new
        {
            type = exception.GetType().Name,
            message = exception.Message,
            stackTrace = exception.StackTrace,
            innerException = GetInnerExceptionInfo(exception.InnerException)
        };
    }
}