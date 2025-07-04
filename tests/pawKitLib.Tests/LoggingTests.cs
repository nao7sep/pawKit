using Microsoft.Extensions.Logging;
using PawKitLib.Logging.Configuration;
using PawKitLib.Logging.Core;
using PawKitLib.Logging.Destinations.Console;
using PawKitLib.Logging.Destinations.File;
using PawKitLib.Logging.Destinations.Database;
using PawKitLib.Logging.Loggers;
using PawKitLib.Logging.Structured;
using System.Text.Json;
using Xunit;

namespace PawKitLib.Tests;

public class LoggingTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _tempFiles;

    public LoggingTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "PawKitTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _tempFiles = new List<string>();
    }

    public void Dispose()
    {
        // Clean up test files
        foreach (var file in _tempFiles)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch { }
        }

        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
        catch { }
    }

    [Fact]
    public void LoggerConfiguration_Build_WithNoDestinations_ThrowsException()
    {
        // Arrange
        var config = LoggerConfiguration.Create();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => config.Build());
        Assert.Contains("At least one log destination must be configured", exception.Message);
    }

    [Fact]
    public void LoggerConfiguration_Build_WithDestinations_ReturnsFactory()
    {
        // Arrange
        var config = LoggerConfiguration.Create()
            .AddPawKitConsole();

        // Act
        using var factory = config.Build();

        // Assert
        Assert.NotNull(factory);
        Assert.IsType<PawKitLoggerFactory>(factory);
    }

    [Fact]
    public void LoggerConfiguration_SetMinimumLevel_SetsCorrectLevel()
    {
        // Arrange & Act
        using var factory = LoggerConfiguration.Create()
            .SetMinimumLevel(LogLevel.Warning)
            .AddPawKitConsole()
            .Build();

        var logger = factory.CreateLogger("Test");

        // Assert
        Assert.False(logger.IsEnabled(LogLevel.Information));
        Assert.True(logger.IsEnabled(LogLevel.Warning));
        Assert.True(logger.IsEnabled(LogLevel.Error));
    }

    [Fact]
    public void LoggerConfiguration_AddPawKitPlainText_WithInvalidPath_ThrowsException()
    {
        // Arrange
        var config = LoggerConfiguration.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => config.AddPawKitPlainText(""));
        Assert.Throws<ArgumentException>(() => config.AddPawKitPlainText(null!));
        Assert.Throws<ArgumentException>(() => config.AddPawKitPlainText("   "));
    }

    [Fact]
    public void PawKitLogger_Log_WithEnabledLevel_WritesToDestinations()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "test.log");
        _tempFiles.Add(testFilePath);

        using var factory = LoggerConfiguration.Create()
            .SetMinimumLevel(LogLevel.Information)
            .AddPawKitPlainText(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");
        factory.Flush();

        // Assert
        Assert.True(File.Exists(testFilePath));
        var content = File.ReadAllText(testFilePath);
        Assert.Contains("Test message", content);
        Assert.Contains("TestCategory", content);
        Assert.Contains("[INFO]", content);
    }

    [Fact]
    public void PawKitLogger_Log_WithDisabledLevel_DoesNotWrite()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "test.log");
        _tempFiles.Add(testFilePath);

        using var factory = LoggerConfiguration.Create()
            .SetMinimumLevel(LogLevel.Warning)
            .AddPawKitPlainText(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");
        factory.Flush();

        // Assert
        if (File.Exists(testFilePath))
        {
            var content = File.ReadAllText(testFilePath);
            Assert.DoesNotContain("Test message", content);
        }
    }

    [Fact]
    public void PawKitLogger_LogWithException_IncludesExceptionDetails()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "test.log");
        _tempFiles.Add(testFilePath);

        using var factory = LoggerConfiguration.Create()
            .AddPawKitPlainText(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");
        var exception = new InvalidOperationException("Test exception");

        // Act
        logger.LogError(exception, "Error occurred");
        factory.Flush();

        // Assert
        var content = File.ReadAllText(testFilePath);
        Assert.Contains("Error occurred", content);
        Assert.Contains("Test exception", content);
        Assert.Contains("InvalidOperationException", content);
    }

    [Fact]
    public void JsonFileLogDestination_WritesValidJson()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "test.json");
        _tempFiles.Add(testFilePath);

        using var factory = LoggerConfiguration.Create()
            .AddPawKitJson(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");
        factory.Flush();

        // Assert
        var content = File.ReadAllText(testFilePath);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            // Each line should be valid JSON
            var jsonDoc = JsonDocument.Parse(line);
            Assert.True(jsonDoc.RootElement.TryGetProperty("@timestamp", out _));
            Assert.Equal("Information", jsonDoc.RootElement.GetProperty("@level").GetString());
            Assert.Equal("TestCategory", jsonDoc.RootElement.GetProperty("@category").GetString());
            Assert.Equal("Test message", jsonDoc.RootElement.GetProperty("@message").GetString());
        }
    }

    [Fact]
    public void SqliteLogDestination_CreatesTableAndWritesData()
    {
        // Arrange
        var testDbPath = Path.Combine(_testDirectory, "test.db");
        _tempFiles.Add(testDbPath);

        using var factory = LoggerConfiguration.Create()
            .AddPawKitSqlite(testDbPath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Test message");
        factory.Flush();

        // Assert
        Assert.True(File.Exists(testDbPath));

        // Verify data was written
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={testDbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM LogEntries WHERE Message = 'Test message'";
        var count = Convert.ToInt32(command.ExecuteScalar());
        Assert.Equal(1, count);
    }

    [Fact]
    public void StructuredLogging_ExtractsProperties()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "structured.json");
        _tempFiles.Add(testFilePath);

        using var factory = LoggerConfiguration.Create()
            .AddPawKitJson(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformationStructured("User {UserId} logged in from {IpAddress}", 123, "192.168.1.1");
        factory.Flush();

        // Assert
        var content = File.ReadAllText(testFilePath);
        var jsonDoc = JsonDocument.Parse(content.Trim());

        Assert.Equal(123, jsonDoc.RootElement.GetProperty("@UserId").GetInt32());
        Assert.Equal("192.168.1.1", jsonDoc.RootElement.GetProperty("@IpAddress").GetString());
        Assert.Contains("User 123 logged in from 192.168.1.1", jsonDoc.RootElement.GetProperty("@message").GetString());
    }

    [Fact]
    public void LogScope_CapturesProperties()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "scope.json");
        _tempFiles.Add(testFilePath);

        using var factory = LoggerConfiguration.Create()
            .AddPawKitJson(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        using (logger.BeginStructuredScope("RequestId", "12345"))
        {
            logger.LogInformation("Processing request");
        }
        factory.Flush();

        // Assert
        var content = File.ReadAllText(testFilePath);
        var jsonDoc = JsonDocument.Parse(content.Trim());

        Assert.Equal("12345", jsonDoc.RootElement.GetProperty("scope.RequestId").GetString());
    }

    [Fact]
    public void BufferedDestination_FlushesAutomatically()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "buffered.log");
        _tempFiles.Add(testFilePath);

        using var factory = LoggerConfiguration.Create()
            .AddPawKitPlainText(testFilePath, LogWriteMode.Buffered)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act - Write enough messages to trigger auto-flush
        for (int i = 0; i < 150; i++) // More than default buffer size of 100
        {
            logger.LogInformation($"Message {i}");
        }

        // Assert - Should have auto-flushed without explicit flush call
        var content = File.ReadAllText(testFilePath);
        Assert.Contains("Message 0", content);
        Assert.Contains("Message 99", content);
    }

    [Fact]
    public async Task ThreadSafeDestination_HandlesMultipleThreads()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "threadsafe.log");
        _tempFiles.Add(testFilePath);

        using var factory = LoggerConfiguration.Create()
            .AddPawKitPlainText(testFilePath, LogWriteMode.Immediate, LogThreadSafety.ThreadSafe)
            .Build();

        var logger = factory.CreateLogger("TestCategory");
        var tasks = new List<Task>();

        // Act - Write from multiple threads
        for (int i = 0; i < 10; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 10; j++)
                {
                    logger.LogInformation($"Thread {threadId} Message {j}");
                }
            }));
        }

        await Task.WhenAll(tasks.ToArray());
        factory.Flush();

        // Assert - All messages should be present
        var content = File.ReadAllText(testFilePath);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(100, lines.Length); // 10 threads * 10 messages each
    }

    [Fact]
    public void LoggerConfiguration_AddPawKitPlainText_WithInvalidCharacters_ThrowsException()
    {
        // Arrange
        var config = LoggerConfiguration.Create();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => config.AddPawKitPlainText("test<>file.log"));
        Assert.Throws<ArgumentException>(() => config.AddPawKitPlainText("test|file.log"));
    }

    [Fact]
    public void LoggerConfiguration_AddPawKitSqlite_WithConnectionPooling_Works()
    {
        // Arrange
        var testDbPath = Path.Combine(_testDirectory, "pooled_test.db");
        _tempFiles.Add(testDbPath);

        // Act
        using var factory = LoggerConfiguration.Create()
            .AddPawKitSqlite(testDbPath, maxPoolSize: 5)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Log multiple messages to test pooling
        for (int i = 0; i < 10; i++)
        {
            logger.LogInformation($"Pooled message {i}");
        }
        factory.Flush();

        // Assert
        Assert.True(File.Exists(testDbPath));

        // Verify all messages were written
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={testDbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM LogEntries";
        var count = Convert.ToInt32(command.ExecuteScalar());
        Assert.Equal(10, count);
    }
}