using Microsoft.Extensions.Logging;

namespace PawKitLib.Logging;

/// <summary>
/// Represents a log entry with all necessary information for logging.
/// </summary>
public sealed class LogEntry
{
    /// <summary>
    /// Gets the UTC timestamp when the log entry was created.
    /// </summary>
    public DateTime TimestampUtc { get; }

    /// <summary>
    /// Gets the log level of the entry.
    /// </summary>
    public LogLevel LogLevel { get; }

    /// <summary>
    /// Gets the category name of the logger.
    /// </summary>
    public string CategoryName { get; }

    /// <summary>
    /// Gets the event ID associated with the log entry.
    /// </summary>
    public EventId EventId { get; }

    /// <summary>
    /// Gets the formatted log message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the exception associated with the log entry, if any.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Initializes a new instance of the LogEntry class.
    /// </summary>
    public LogEntry(DateTime timestampUtc, LogLevel logLevel, string categoryName, EventId eventId, string message, Exception? exception = null)
    {
        TimestampUtc = timestampUtc;
        LogLevel = logLevel;
        CategoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
        EventId = eventId;
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Exception = exception;
    }
}