using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;

namespace PawKitLib.Logging;

/// <summary>
/// A log destination that writes log entries to a SQLite database.
/// </summary>
public sealed class SqliteLogDestination : BaseLogDestination
{
    private readonly string _connectionString;
    private readonly bool _createIfNotExists;

    /// <summary>
    /// Initializes a new instance of the SqliteLogDestination class.
    /// </summary>
    /// <param name="filePath">The path to the SQLite database file.</param>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="createIfNotExists">Whether to create the database and table if they don't exist.</param>
    public SqliteLogDestination(string filePath, LogWriteMode writeMode, LogThreadSafety threadSafety, bool createIfNotExists = true)
        : base(writeMode, threadSafety)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        _createIfNotExists = createIfNotExists;
        _connectionString = $"Data Source={filePath}";

        if (_createIfNotExists)
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Initialize the database and create the table if it doesn't exist
            InitializeDatabase();
        }
    }

    /// <summary>
    /// Writes a single log entry to the SQLite database.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    protected override void WriteLogEntry(LogEntry logEntry)
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO LogEntries (TimestampUtc, LogLevel, CategoryName, EventId, EventName, Message, Exception)
                VALUES (@TimestampUtc, @LogLevel, @CategoryName, @EventId, @EventName, @Message, @Exception)";

            command.Parameters.AddWithValue("@TimestampUtc", logEntry.TimestampUtc.ToString("O")); // ISO 8601 roundtrip format
            command.Parameters.AddWithValue("@LogLevel", logEntry.LogLevel.ToString());
            command.Parameters.AddWithValue("@CategoryName", logEntry.CategoryName);
            command.Parameters.AddWithValue("@EventId", logEntry.EventId.Id);
            command.Parameters.AddWithValue("@EventName", logEntry.EventId.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Message", logEntry.Message);
            command.Parameters.AddWithValue("@Exception", logEntry.Exception?.ToString() ?? (object)DBNull.Value);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            // If we can't write to the database, write to console as fallback
            Console.WriteLine($"Failed to write to SQLite log database: {ex.Message}");
            Console.WriteLine($"Log entry: {logEntry.Message}");
        }
    }


    /// <summary>
    /// Initializes the SQLite database and creates the log table if it doesn't exist.
    /// </summary>
    private void InitializeDatabase()
    {
        try
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS LogEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TimestampUtc TEXT NOT NULL,
                    LogLevel TEXT NOT NULL,
                    CategoryName TEXT NOT NULL,
                    EventId INTEGER NOT NULL,
                    EventName TEXT,
                    Message TEXT NOT NULL,
                    Exception TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            command.ExecuteNonQuery();

            // Create an index on TimestampUtc for better query performance
            command.CommandText = @"
                CREATE INDEX IF NOT EXISTS IX_LogEntries_TimestampUtc
                ON LogEntries (TimestampUtc)";

            command.ExecuteNonQuery();

            // Create an index on LogLevel for filtering
            command.CommandText = @"
                CREATE INDEX IF NOT EXISTS IX_LogEntries_LogLevel
                ON LogEntries (LogLevel)";

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize SQLite log database: {ex.Message}", ex);
        }
    }
}