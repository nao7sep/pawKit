using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PawKitLib.Logging;
using Xunit;

namespace pawKitLib.Tests;

/// <summary>
/// Comprehensive tests for the PawKit logging system.
/// Tests are organized from basic components to complex integrations.
/// </summary>
public class LoggingTests
{
    #region Helper Methods

    /// <summary>
    /// Helper method to safely delete a file with retry logic for SQLite database files.
    /// </summary>
    /// <param name="filePath">The path to the file to delete.</param>
    private static void TryDeleteFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        for (int i = 0; i < 5; i++)
        {
            try
            {
                File.Delete(filePath);
                return;
            }
            catch (IOException)
            {
                // Wait a bit and try again
                Thread.Sleep(100);
            }
        }

        // If we still can't delete it, just ignore it
        // The temp file will be cleaned up by the OS eventually
    }

    /// <summary>
    /// Creates a temporary SQLite database file path.
    /// </summary>
    /// <returns>A temporary database file path with .db extension.</returns>
    private static string CreateTempDbPath()
    {
        var tempDbFile = Path.GetTempFileName();
        File.Delete(tempDbFile); // Delete the temp file so SQLite can create it properly
        return Path.ChangeExtension(tempDbFile, ".db");
    }

    #endregion

    #region Core Component Tests

    [Fact]
    public void LogEntry_Construction_ShouldSetAllProperties()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var eventId = new EventId(1, "TestEvent");
        var exception = new InvalidOperationException("Test exception");

        // Act
        var logEntry = new LogEntry(
            timestampUtc: utcNow,
            logLevel: LogLevel.Error,
            categoryName: "TestCategory",
            eventId: eventId,
            message: "Test message",
            exception: exception
        );

        // Assert
        Assert.Equal(utcNow, logEntry.TimestampUtc);
        Assert.Equal(LogLevel.Error, logEntry.LogLevel);
        Assert.Equal("TestCategory", logEntry.CategoryName);
        Assert.Equal(eventId, logEntry.EventId);
        Assert.Equal("Test message", logEntry.Message);
        Assert.Equal(exception, logEntry.Exception);
    }

    [Fact]
    public void LogEntry_TimestampUtc_ShouldUseRoundtripFormat()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var logEntry = new LogEntry(
            timestampUtc: utcNow,
            logLevel: LogLevel.Information,
            categoryName: "TestCategory",
            eventId: new EventId(1, "TestEvent"),
            message: "Test message"
        );

        // Act
        var roundtripFormatted = logEntry.TimestampUtc.ToString("O");

        // Assert
        Assert.Equal(utcNow, logEntry.TimestampUtc);
        Assert.True(DateTime.TryParseExact(roundtripFormatted, "O", null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedDate));
        Assert.Equal(utcNow, parsedDate);
        Assert.Contains("Z", roundtripFormatted); // Should contain UTC indicator
    }

    [Fact]
    public void LogEntry_PropertyName_ShouldIndicateUtc()
    {
        // Arrange
        var utcNow = DateTime.UtcNow;
        var logEntry = new LogEntry(
            timestampUtc: utcNow,
            logLevel: LogLevel.Information,
            categoryName: "TestCategory",
            eventId: new EventId(1, "TestEvent"),
            message: "Test message"
        );

        // Act & Assert
        // Verify the property name clearly indicates UTC
        var propertyInfo = typeof(LogEntry).GetProperty("TimestampUtc");
        Assert.NotNull(propertyInfo);
        Assert.Equal(utcNow, logEntry.TimestampUtc);
    }

    #endregion

    #region Configuration Tests

    [Fact]
    public void LoggerConfiguration_NoDestinations_ShouldThrowException()
    {
        // Arrange
        var config = LoggerConfiguration.Create();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => config.Build());
    }

    [Fact]
    public void LoggerConfiguration_MinimumLevel_ShouldFilterLogs()
    {
        // Arrange
        var tempLogFile = Path.GetTempFileName();

        try
        {
            var config = LoggerConfiguration.Create()
                .SetMinimumLevel(LogLevel.Warning)
                .AddPawKitPlainText(tempLogFile, LogWriteMode.Immediate, LogThreadSafety.ThreadSafe);

            using var loggerFactory = config.Build();
            var logger = loggerFactory.CreateLogger("TestLogger");

            // Act
            logger.LogDebug("Debug message - should be filtered");
            logger.LogInformation("Info message - should be filtered");
            logger.LogWarning("Warning message - should appear");
            logger.LogError("Error message - should appear");

            // Assert
            Assert.True(File.Exists(tempLogFile));
            var logContent = File.ReadAllText(tempLogFile);
            Assert.DoesNotContain("Debug message", logContent);
            Assert.DoesNotContain("Info message", logContent);
            Assert.Contains("Warning message", logContent);
            Assert.Contains("Error message", logContent);
        }
        finally
        {
            if (File.Exists(tempLogFile)) File.Delete(tempLogFile);
        }
    }

    #endregion

    #region Write Mode Tests

    [Fact]
    public void LoggerConfiguration_BufferedMode_ShouldFlushOnDispose()
    {
        // Arrange
        var tempLogFile = Path.GetTempFileName();

        try
        {
            var config = LoggerConfiguration.Create()
                .AddPawKitPlainText(tempLogFile, LogWriteMode.Buffered, LogThreadSafety.ThreadSafe);

            using var loggerFactory = config.Build();
            var logger = loggerFactory.CreateLogger("TestLogger");

            // Act
            logger.LogInformation("Buffered message");

            // Before dispose, file might be empty due to buffering
            var contentBeforeDispose = File.Exists(tempLogFile) ? File.ReadAllText(tempLogFile) : "";

            // Dispose should flush the buffer
            loggerFactory.Dispose();

            // Assert
            Assert.True(File.Exists(tempLogFile));
            var contentAfterDispose = File.ReadAllText(tempLogFile);
            Assert.Contains("Buffered message", contentAfterDispose);
        }
        finally
        {
            if (File.Exists(tempLogFile)) File.Delete(tempLogFile);
        }
    }

    [Fact]
    public void SqliteLogDestination_BufferedMode_ShouldFlushOnDispose()
    {
        // Arrange
        var tempDbFile = CreateTempDbPath();

        try
        {
            var config = LoggerConfiguration.Create()
                .AddPawKitSqlite(tempDbFile, LogWriteMode.Buffered, LogThreadSafety.ThreadSafe);

            using var loggerFactory = config.Build();
            var logger = loggerFactory.CreateLogger("TestLogger");

            // Act
            logger.LogInformation("Buffered SQLite message");

            // Before dispose, check if data might not be written yet due to buffering
            using (var connection = new SqliteConnection($"Data Source={tempDbFile}"))
            {
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM LogEntries";
                var countBeforeDispose = Convert.ToInt32(command.ExecuteScalar());
                // Count might be 0 or 1 depending on buffer size and timing
            }

            // Dispose should flush the buffer
            loggerFactory.Dispose();

            // Assert
            Assert.True(File.Exists(tempDbFile));
            using var connectionAfter = new SqliteConnection($"Data Source={tempDbFile}");
            connectionAfter.Open();
            using var commandAfter = connectionAfter.CreateCommand();
            commandAfter.CommandText = "SELECT COUNT(*) FROM LogEntries WHERE Message = 'Buffered SQLite message'";
            var countAfterDispose = Convert.ToInt32(commandAfter.ExecuteScalar());
            Assert.Equal(1, countAfterDispose);
        }
        finally
        {
            // Force garbage collection to help release SQLite connections
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            TryDeleteFile(tempDbFile);
        }
    }

    #endregion

    #region SQLite Destination Tests

    [Fact]
    public void SqliteLogDestination_DatabaseSchema_ShouldBeCreatedCorrectly()
    {
        // Arrange
        var tempDbFile = CreateTempDbPath();

        try
        {
            // Act
            var destination = new SqliteLogDestination(tempDbFile, LogWriteMode.Immediate, LogThreadSafety.ThreadSafe);

            // Assert
            Assert.True(File.Exists(tempDbFile));

            using var connection = new SqliteConnection($"Data Source={tempDbFile}");
            connection.Open();

            // Check if table exists
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='LogEntries'";
            var tableName = command.ExecuteScalar() as string;
            Assert.Equal("LogEntries", tableName);

            // Check table structure
            command.CommandText = "PRAGMA table_info(LogEntries)";
            using var reader = command.ExecuteReader();
            var columns = new List<string>();
            while (reader.Read())
            {
                columns.Add(reader.GetString(1)); // Column name is at index 1 in PRAGMA table_info
            }

            Assert.Contains("Id", columns);
            Assert.Contains("TimestampUtc", columns);
            Assert.Contains("LogLevel", columns);
            Assert.Contains("CategoryName", columns);
            Assert.Contains("EventId", columns);
            Assert.Contains("EventName", columns);
            Assert.Contains("Message", columns);
            Assert.Contains("Exception", columns);
            Assert.Contains("CreatedAt", columns);

            destination.Dispose();
        }
        finally
        {
            // Force garbage collection to help release SQLite connections
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            TryDeleteFile(tempDbFile);
        }
    }

    [Fact]
    public void SqliteLogDestination_InvalidFilePath_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new SqliteLogDestination("", LogWriteMode.Immediate, LogThreadSafety.ThreadSafe));

        Assert.Throws<ArgumentException>(() =>
            new SqliteLogDestination("   ", LogWriteMode.Immediate, LogThreadSafety.ThreadSafe));
    }

    #endregion

    #region Dependency Injection Integration Tests

    [Fact]
    public void DependencyInjection_BasicLogging_ShouldWork()
    {
        // Arrange
        var tempLogFile = Path.GetTempFileName();

        try
        {
            var services = new ServiceCollection();
            services.AddPawKitLogging(config => config
                .SetMinimumLevel(LogLevel.Debug)
                .AddPawKitConsole(LogWriteMode.Immediate, LogThreadSafety.ThreadSafe, useColors: false)
                .AddPawKitPlainText(tempLogFile, LogWriteMode.Immediate, LogThreadSafety.ThreadSafe));

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var logger = serviceProvider.GetRequiredService<ILogger<LoggingTests>>();
            logger.LogDebug("Debug message");
            logger.LogInformation("Information message");
            logger.LogWarning("Warning message");

            // Assert
            Assert.True(File.Exists(tempLogFile));
            var logContent = File.ReadAllText(tempLogFile);
            Assert.Contains("Debug message", logContent);
            Assert.Contains("Information message", logContent);
            Assert.Contains("Warning message", logContent);

            // Cleanup
            serviceProvider.Dispose();
        }
        finally
        {
            if (File.Exists(tempLogFile)) File.Delete(tempLogFile);
        }
    }

    [Fact]
    public void DependencyInjection_SqliteDestination_ShouldWork()
    {
        // Arrange
        var tempDbFile = CreateTempDbPath();

        try
        {
            var services = new ServiceCollection();
            services.AddPawKitLogging(config => config
                .SetMinimumLevel(LogLevel.Debug)
                .AddPawKitSqlite(tempDbFile, LogWriteMode.Immediate, LogThreadSafety.ThreadSafe));

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var logger = serviceProvider.GetRequiredService<ILogger<LoggingTests>>();
            logger.LogDebug("Debug message for SQLite");
            logger.LogInformation("Information message for SQLite");
            logger.LogWarning("Warning message for SQLite");

            // Assert
            Assert.True(File.Exists(tempDbFile));

            using var connection = new SqliteConnection($"Data Source={tempDbFile}");
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM LogEntries WHERE Message LIKE '%SQLite'";
            var count = Convert.ToInt32(command.ExecuteScalar());
            Assert.Equal(3, count);

            // Cleanup
            serviceProvider.Dispose();
        }
        finally
        {
            // Force garbage collection to help release SQLite connections
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            TryDeleteFile(tempDbFile);
        }
    }

    [Fact]
    public void DependencyInjection_MultipleDestinations_ShouldWriteToAll()
    {
        // Arrange
        var tempLogFile = Path.GetTempFileName();
        var tempJsonFile = Path.GetTempFileName();

        try
        {
            var services = new ServiceCollection();
            services.AddPawKitLogging(config => config
                .SetMinimumLevel(LogLevel.Information)
                .AddPawKitConsole(LogWriteMode.Immediate, LogThreadSafety.ThreadSafe, useColors: false)
                .AddPawKitPlainText(tempLogFile, LogWriteMode.Immediate, LogThreadSafety.ThreadSafe)
                .AddPawKitJson(tempJsonFile, LogWriteMode.Immediate, LogThreadSafety.ThreadSafe));

            var serviceProvider = services.BuildServiceProvider();

            // Act
            var logger = serviceProvider.GetRequiredService<ILogger<LoggingTests>>();
            logger.LogInformation("Test information message");
            logger.LogWarning("Test warning message");
            logger.LogError(new InvalidOperationException("Test exception"), "Test error message");

            // Assert
            Assert.True(File.Exists(tempLogFile));
            Assert.True(File.Exists(tempJsonFile));

            var logContent = File.ReadAllText(tempLogFile);
            Assert.Contains("Test information message", logContent);
            Assert.Contains("Test warning message", logContent);
            Assert.Contains("Test error message", logContent);

            var jsonContent = File.ReadAllText(tempJsonFile);
            Assert.Contains("Test information message", jsonContent);
            Assert.Contains("Test warning message", jsonContent);
            Assert.Contains("Test error message", jsonContent);

            // Cleanup
            serviceProvider.Dispose();
        }
        finally
        {
            if (File.Exists(tempLogFile)) File.Delete(tempLogFile);
            if (File.Exists(tempJsonFile)) File.Delete(tempJsonFile);
        }
    }

    #endregion
}