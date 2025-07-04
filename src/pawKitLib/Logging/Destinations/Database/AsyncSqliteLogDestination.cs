using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using PawKitLib.Data.Sqlite;
using PawKitLib.Logging.Core;
using PawKitLib.Logging.Destinations.Base;

namespace PawKitLib.Logging.Destinations.Database;

/// <summary>
/// An asynchronous log destination that writes log entries to a SQLite database.
/// </summary>
public sealed class AsyncSqliteLogDestination : BaseAsyncLogDestination
{
    private readonly SqliteConnectionPool _connectionPool;
    private readonly bool _createIfNotExists;

    /// <summary>
    /// Initializes a new instance of the AsyncSqliteLogDestination class.
    /// </summary>
    /// <param name="filePath">The path to the SQLite database file.</param>
    /// <param name="writeMode">The write mode for this destination.</param>
    /// <param name="threadSafety">The thread safety mode for this destination.</param>
    /// <param name="createIfNotExists">Whether to create the database and table if they don't exist.</param>
    /// <param name="maxPoolSize">The maximum number of connections to pool. Default is 10.</param>
    public AsyncSqliteLogDestination(string filePath, LogWriteMode writeMode, LogThreadSafety threadSafety, bool createIfNotExists = true, int maxPoolSize = 10)
        : base(writeMode, threadSafety)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

        _createIfNotExists = createIfNotExists;
        var connectionString = $"Data Source={filePath}";
        _connectionPool = new SqliteConnectionPool(connectionString, maxPoolSize);

        if (_createIfNotExists)
        {
            // Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Initialize the database and create the table if it doesn't exist
            InitializeDatabaseAsync().GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Asynchronously writes a single log entry to the SQLite database.
    /// </summary>
    /// <param name="logEntry">The log entry to write.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    protected override async Task WriteLogEntryAsync(LogEntry logEntry, CancellationToken cancellationToken)
    {
        try
        {
            using var pooledConnection = await _connectionPool.GetConnectionAsync(cancellationToken).ConfigureAwait(false);
            using var command = pooledConnection.Connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO LogEntries (TimestampUtc, LogLevel, CategoryName, EventId, EventName, Message, MessageTemplate, Properties, ScopeProperties, Exception)
                VALUES (@TimestampUtc, @LogLevel, @CategoryName, @EventId, @EventName, @Message, @MessageTemplate, @Properties, @ScopeProperties, @Exception)";

            command.Parameters.AddWithValue("@TimestampUtc", logEntry.TimestampUtc.ToString("O")); // ISO 8601 roundtrip format
            command.Parameters.AddWithValue("@LogLevel", logEntry.LogLevel.ToString());
            command.Parameters.AddWithValue("@CategoryName", logEntry.CategoryName);
            command.Parameters.AddWithValue("@EventId", logEntry.EventId.Id);
            command.Parameters.AddWithValue("@EventName", logEntry.EventId.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Message", logEntry.Message);
            command.Parameters.AddWithValue("@MessageTemplate", logEntry.MessageTemplate ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Properties", SerializeProperties(logEntry.Properties));
            command.Parameters.AddWithValue("@ScopeProperties", SerializeProperties(logEntry.ScopeProperties));
            command.Parameters.AddWithValue("@Exception", logEntry.Exception?.ToString() ?? (object)DBNull.Value);

            await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // If we can't write to the database, write to console as fallback
            await System.Console.Out.WriteLineAsync($"Failed to write to SQLite log database: {ex.Message}").ConfigureAwait(false);
            await System.Console.Out.WriteLineAsync($"Log entry: {logEntry.Message}").ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Asynchronously initializes the SQLite database and creates the log table if it doesn't exist.
    /// </summary>
    private async Task InitializeDatabaseAsync()
    {
        try
        {
            using var pooledConnection = await _connectionPool.GetConnectionAsync().ConfigureAwait(false);
            using var command = pooledConnection.Connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS LogEntries (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    TimestampUtc TEXT NOT NULL,
                    LogLevel TEXT NOT NULL,
                    CategoryName TEXT NOT NULL,
                    EventId INTEGER NOT NULL,
                    EventName TEXT,
                    Message TEXT NOT NULL,
                    MessageTemplate TEXT,
                    Properties TEXT,
                    ScopeProperties TEXT,
                    Exception TEXT,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                )";

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            // Create an index on TimestampUtc for better query performance
            command.CommandText = @"
                CREATE INDEX IF NOT EXISTS IX_LogEntries_TimestampUtc
                ON LogEntries (TimestampUtc)";

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            // Create an index on LogLevel for filtering
            command.CommandText = @"
                CREATE INDEX IF NOT EXISTS IX_LogEntries_LogLevel
                ON LogEntries (LogLevel)";

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);

            // Create an index on CategoryName for filtering
            command.CommandText = @"
                CREATE INDEX IF NOT EXISTS IX_LogEntries_CategoryName
                ON LogEntries (CategoryName)";

            await command.ExecuteNonQueryAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize SQLite log database: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Serializes properties to JSON string for storage.
    /// </summary>
    /// <param name="properties">The properties to serialize.</param>
    /// <returns>JSON string or DBNull if no properties.</returns>
    private static object SerializeProperties(IReadOnlyDictionary<string, object?> properties)
    {
        if (properties.Count == 0)
            return DBNull.Value;

        try
        {
            return System.Text.Json.JsonSerializer.Serialize(properties);
        }
        catch
        {
            return DBNull.Value;
        }
    }

    /// <summary>
    /// Asynchronously disposes the connection pool when the destination is disposed.
    /// </summary>
    public new async ValueTask DisposeAsync()
    {
        _connectionPool?.Dispose();
        await base.DisposeAsync().ConfigureAwait(false);
    }
}