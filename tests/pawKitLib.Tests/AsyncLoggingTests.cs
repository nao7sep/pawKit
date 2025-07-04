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

public class AsyncLoggingTests : IAsyncDisposable
{
    private readonly string _testDirectory;
    private readonly List<string> _tempFiles;

    public AsyncLoggingTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "PawKitAsyncTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        _tempFiles = new List<string>();
    }

    public async ValueTask DisposeAsync()
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
    public void AsyncLoggerConfiguration_Build_WithNoDestinations_ThrowsException()
    {
        // Arrange
        var config = AsyncLoggerConfiguration.Create();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => config.Build());
        Assert.Contains("At least one async log destination must be configured", exception.Message);
    }

    [Fact]
    public async Task AsyncLoggerConfiguration_Build_WithDestinations_ReturnsFactory()
    {
        // Arrange
        var config = AsyncLoggerConfiguration.Create()
            .AddAsyncConsole();

        // Act
        await using var factory = config.Build();

        // Assert
        Assert.NotNull(factory);
        Assert.IsType<AsyncPawKitLoggerFactory>(factory);
    }

    [Fact]
    public async Task AsyncPawKitLogger_Log_WithEnabledLevel_WritesToDestinations()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_test.log");
        _tempFiles.Add(testFilePath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .SetMinimumLevel(LogLevel.Information)
            .AddAsyncPlainText(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Async test message");
        await factory.FlushAsync();

        // Assert
        Assert.True(File.Exists(testFilePath));
        var content = await File.ReadAllTextAsync(testFilePath);
        Assert.Contains("Async test message", content);
        Assert.Contains("TestCategory", content);
        Assert.Contains("[INFO]", content);
    }

    [Fact]
    public async Task AsyncPawKitLogger_Log_WithDisabledLevel_DoesNotWrite()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_test.log");
        _tempFiles.Add(testFilePath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .SetMinimumLevel(LogLevel.Warning)
            .AddAsyncPlainText(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Async test message");
        await factory.FlushAsync();

        // Assert
        if (File.Exists(testFilePath))
        {
            var content = await File.ReadAllTextAsync(testFilePath);
            Assert.DoesNotContain("Async test message", content);
        }
    }

    [Fact]
    public async Task AsyncJsonFileLogDestination_WritesValidJson()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_test.json");
        _tempFiles.Add(testFilePath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncJson(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Async test message");
        await factory.FlushAsync();

        // Assert
        var content = await File.ReadAllTextAsync(testFilePath);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var jsonDoc = JsonDocument.Parse(line);
            Assert.NotNull(jsonDoc.RootElement.GetProperty("@timestamp"));
            Assert.Equal("Information", jsonDoc.RootElement.GetProperty("@level").GetString());
            Assert.Equal("TestCategory", jsonDoc.RootElement.GetProperty("@category").GetString());
            Assert.Equal("Async test message", jsonDoc.RootElement.GetProperty("@message").GetString());
        }
    }

    [Fact]
    public async Task AsyncSqliteLogDestination_CreatesTableAndWritesData()
    {
        // Arrange
        var testDbPath = Path.Combine(_testDirectory, "async_test.db");
        _tempFiles.Add(testDbPath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncSqlite(testDbPath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformation("Async test message");
        await factory.FlushAsync();

        // Assert
        Assert.True(File.Exists(testDbPath));

        // Verify data was written
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={testDbPath}");
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM LogEntries WHERE Message = 'Async test message'";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task AsyncStructuredLogging_ExtractsProperties()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_structured.json");
        _tempFiles.Add(testFilePath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncJson(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        logger.LogInformationStructured("Async user {UserId} logged in from {IpAddress}", 456, "10.0.0.1");
        await factory.FlushAsync();

        // Assert
        var content = await File.ReadAllTextAsync(testFilePath);
        var jsonDoc = JsonDocument.Parse(content.Trim());

        Assert.Equal("456", jsonDoc.RootElement.GetProperty("@UserId").GetString());
        Assert.Equal("10.0.0.1", jsonDoc.RootElement.GetProperty("@IpAddress").GetString());
        Assert.Contains("Async user 456 logged in from 10.0.0.1", jsonDoc.RootElement.GetProperty("@message").GetString());
    }

    [Fact]
    public async Task AsyncLogScope_CapturesProperties()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_scope.json");
        _tempFiles.Add(testFilePath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncJson(testFilePath)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act
        using (logger.BeginStructuredScope("CorrelationId", "async-67890"))
        {
            logger.LogInformation("Processing async request");
        }
        await factory.FlushAsync();

        // Assert
        var content = await File.ReadAllTextAsync(testFilePath);
        var jsonDoc = JsonDocument.Parse(content.Trim());

        Assert.Equal("async-67890", jsonDoc.RootElement.GetProperty("scope.CorrelationId").GetString());
    }

    [Fact]
    public async Task AsyncBufferedDestination_FlushesAutomatically()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_buffered.log");
        _tempFiles.Add(testFilePath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncPlainText(testFilePath, LogWriteMode.Buffered)
            .Build();

        var logger = factory.CreateLogger("TestCategory");

        // Act - Write enough messages to trigger auto-flush
        for (int i = 0; i < 150; i++)
        {
            logger.LogInformation($"Async message {i}");
        }

        // Wait a bit for background processing
        await Task.Delay(100);

        // Assert - Should have auto-flushed
        var content = await File.ReadAllTextAsync(testFilePath);
        Assert.Contains("Async message 0", content);
        Assert.Contains("Async message 99", content);
    }

    [Fact]
    public async Task AsyncThreadSafeDestination_HandlesMultipleThreads()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_threadsafe.log");
        _tempFiles.Add(testFilePath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncPlainText(testFilePath, LogWriteMode.Immediate, LogThreadSafety.ThreadSafe)
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
                    logger.LogInformation($"Async Thread {threadId} Message {j}");
                }
            }));
        }

        await Task.WhenAll(tasks);
        await factory.FlushAsync();

        // Assert - All messages should be present
        var content = await File.ReadAllTextAsync(testFilePath);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(100, lines.Length);
    }

    [Fact]
    public async Task AsyncLogger_HighVolumeLogging_HandlesLoad()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_highvolume.log");
        _tempFiles.Add(testFilePath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncPlainText(testFilePath, LogWriteMode.Buffered)
            .Build();

        var logger = factory.CreateLogger("HighVolumeTest");
        const int messageCount = 1000;

        // Act - Log many messages quickly
        var tasks = Enumerable.Range(0, 10).Select(async threadId =>
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < messageCount / 10; i++)
                {
                    logger.LogInformation($"High volume message {threadId}-{i}");
                }
            });
        });

        await Task.WhenAll(tasks);
        await factory.FlushAsync();

        // Assert - All messages should be written
        var content = await File.ReadAllTextAsync(testFilePath);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(messageCount, lines.Length);
    }

    [Fact]
    public async Task AsyncLogger_CancellationToken_RespectsCancellation()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_cancellation.log");
        _tempFiles.Add(testFilePath);

        await using var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncPlainText(testFilePath)
            .Build();

        var logger = factory.CreateLogger("CancellationTest");
        using var cts = new CancellationTokenSource();

        // Act
        logger.LogInformation("Message before cancellation");
        cts.Cancel();

        // Flush should handle cancellation gracefully
        await factory.FlushAsync(cts.Token);

        // Assert - Should not throw and should have written the message
        var content = await File.ReadAllTextAsync(testFilePath);
        Assert.Contains("Message before cancellation", content);
    }

    [Fact]
    public async Task AsyncLogger_DisposalHandling_FlushesBeforeDispose()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "async_disposal.log");
        _tempFiles.Add(testFilePath);

        var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncPlainText(testFilePath, LogWriteMode.Buffered)
            .Build();

        var logger = factory.CreateLogger("DisposalTest");

        // Act
        logger.LogInformation("Message before disposal");

        // Dispose should flush automatically
        await factory.DisposeAsync();

        // Assert - Message should be written even without explicit flush
        var content = await File.ReadAllTextAsync(testFilePath);
        Assert.Contains("Message before disposal", content);
    }

    [Fact]
    public async Task AsyncLoggerConfiguration_SetChannelCapacity_ConfiguresCorrectly()
    {
        // Arrange
        var testFilePath = Path.Combine(_testDirectory, "channel_capacity.log");
        _tempFiles.Add(testFilePath);

        // Act
        await using var factory = AsyncLoggerConfiguration.Create()
            .SetChannelCapacity(500)
            .AddAsyncPlainText(testFilePath)
            .Build();

        var logger = factory.CreateLogger("ChannelTest");

        // Log messages and verify they're processed
        for (int i = 0; i < 100; i++)
        {
            logger.LogInformation($"Channel message {i}");
        }

        await factory.FlushAsync();

        // Assert
        var content = await File.ReadAllTextAsync(testFilePath);
        var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(100, lines.Length);
    }

    [Fact]
    public void AsyncLoggerConfiguration_SetChannelCapacity_WithInvalidCapacity_ThrowsException()
    {
        // Arrange
        var config = AsyncLoggerConfiguration.Create();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => config.SetChannelCapacity(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => config.SetChannelCapacity(-1));
    }

    [Fact]
    public async Task AsyncLoggerConfiguration_AddAsyncSqlite_WithConnectionPooling_Works()
    {
        // Arrange
        var testDbPath = Path.Combine(_testDirectory, "async_pooled_test.db");
        _tempFiles.Add(testDbPath);

        // Act
        await using var factory = AsyncLoggerConfiguration.Create()
            .AddAsyncSqlite(testDbPath, maxPoolSize: 3)
            .Build();

        var logger = factory.CreateLogger("AsyncPoolTest");

        // Log multiple messages concurrently to test pooling
        var tasks = Enumerable.Range(0, 15).Select(async i =>
        {
            await Task.Run(() => logger.LogInformation($"Async pooled message {i}"));
        });

        await Task.WhenAll(tasks);
        await factory.FlushAsync();

        // Assert
        Assert.True(File.Exists(testDbPath));

        // Verify all messages were written
        using var connection = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={testDbPath}");
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM LogEntries";
        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        Assert.Equal(15, count);
    }
}