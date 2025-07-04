using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PawKitLib.Logging.Core;
using PawKitLib.Logging.Loggers;
using PawKitLib.Logging.Destinations.Console;
using PawKitLib.Logging.Destinations.File;
using PawKitLib.Logging.Destinations.Database;

namespace PawKitLib.Logging.Configuration;

/// <summary>
/// Extension methods for configuring PawKit logging with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds PawKit logging services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configureLogging">A delegate to configure the logging destinations.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddPawKitLogging(this IServiceCollection services, Action<LoggerConfiguration> configureLogging)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configureLogging == null)
            throw new ArgumentNullException(nameof(configureLogging));

        var configuration = LoggerConfiguration.Create();
        configureLogging(configuration);
        var loggerFactory = configuration.Build();

        // Register the logger factory as singleton
        services.AddSingleton<ILoggerFactory>(loggerFactory);

        // Register generic ILogger<T> factory
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// Adds PawKit logging services to the specified IServiceCollection with a pre-configured logger factory.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="loggerFactory">The pre-configured PawKitLoggerFactory.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddPawKitLogging(this IServiceCollection services, PawKitLoggerFactory loggerFactory)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (loggerFactory == null)
            throw new ArgumentNullException(nameof(loggerFactory));

        // Register the logger factory as singleton
        services.AddSingleton<ILoggerFactory>(loggerFactory);

        // Register generic ILogger<T> factory
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// Adds PawKit asynchronous logging services to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="configureLogging">A delegate to configure the async logging destinations.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddPawKitAsyncLogging(this IServiceCollection services, Action<AsyncLoggerConfiguration> configureLogging)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (configureLogging == null)
            throw new ArgumentNullException(nameof(configureLogging));

        var configuration = AsyncLoggerConfiguration.Create();
        configureLogging(configuration);
        var loggerFactory = configuration.Build();

        // Register the async logger factory as singleton
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton<AsyncPawKitLoggerFactory>(loggerFactory);

        // Register generic ILogger<T> factory
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// Adds PawKit asynchronous logging services to the specified IServiceCollection with a pre-configured logger factory.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="loggerFactory">The pre-configured AsyncPawKitLoggerFactory.</param>
    /// <returns>The IServiceCollection so that additional calls can be chained.</returns>
    public static IServiceCollection AddPawKitAsyncLogging(this IServiceCollection services, AsyncPawKitLoggerFactory loggerFactory)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));
        if (loggerFactory == null)
            throw new ArgumentNullException(nameof(loggerFactory));

        // Register the async logger factory as singleton
        services.AddSingleton<ILoggerFactory>(loggerFactory);
        services.AddSingleton<AsyncPawKitLoggerFactory>(loggerFactory);

        // Register generic ILogger<T> factory
        services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));

        return services;
    }

    /// <summary>
    /// Generic logger implementation that wraps the logger factory.
    /// </summary>
    /// <typeparam name="T">The type to create a logger for.</typeparam>
    private sealed class Logger<T> : ILogger<T>
    {
        private readonly ILogger _logger;

        public Logger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<T>();
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            return _logger.BeginScope(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _logger.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _logger.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}