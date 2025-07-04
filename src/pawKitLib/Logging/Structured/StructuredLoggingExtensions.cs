using Microsoft.Extensions.Logging;

namespace PawKitLib.Logging.Structured;

/// <summary>
/// Extension methods for structured logging.
/// </summary>
public static class StructuredLoggingExtensions
{
    /// <summary>
    /// Logs a structured message with properties.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="logLevel">The log level.</param>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public static void LogStructured(this ILogger logger, LogLevel logLevel, string messageTemplate, params object?[] args)
    {
        if (!logger.IsEnabled(logLevel))
            return;

        logger.Log(logLevel, new EventId(), new StructuredLogState(messageTemplate, args), null, StructuredLogState.Formatter);
    }

    /// <summary>
    /// Logs a structured trace message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public static void LogTraceStructured(this ILogger logger, string messageTemplate, params object?[] args)
    {
        logger.LogStructured(LogLevel.Trace, messageTemplate, args);
    }

    /// <summary>
    /// Logs a structured debug message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public static void LogDebugStructured(this ILogger logger, string messageTemplate, params object?[] args)
    {
        logger.LogStructured(LogLevel.Debug, messageTemplate, args);
    }

    /// <summary>
    /// Logs a structured information message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public static void LogInformationStructured(this ILogger logger, string messageTemplate, params object?[] args)
    {
        logger.LogStructured(LogLevel.Information, messageTemplate, args);
    }

    /// <summary>
    /// Logs a structured warning message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public static void LogWarningStructured(this ILogger logger, string messageTemplate, params object?[] args)
    {
        logger.LogStructured(LogLevel.Warning, messageTemplate, args);
    }

    /// <summary>
    /// Logs a structured error message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public static void LogErrorStructured(this ILogger logger, string messageTemplate, params object?[] args)
    {
        logger.LogStructured(LogLevel.Error, messageTemplate, args);
    }

    /// <summary>
    /// Logs a structured error message with an exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public static void LogErrorStructured(this ILogger logger, Exception exception, string messageTemplate, params object?[] args)
    {
        if (!logger.IsEnabled(LogLevel.Error))
            return;

        logger.Log(LogLevel.Error, new EventId(), new StructuredLogState(messageTemplate, args), exception, StructuredLogState.Formatter);
    }

    /// <summary>
    /// Logs a structured critical message.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public static void LogCriticalStructured(this ILogger logger, string messageTemplate, params object?[] args)
    {
        logger.LogStructured(LogLevel.Critical, messageTemplate, args);
    }

    /// <summary>
    /// Logs a structured critical message with an exception.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public static void LogCriticalStructured(this ILogger logger, Exception exception, string messageTemplate, params object?[] args)
    {
        if (!logger.IsEnabled(LogLevel.Critical))
            return;

        logger.Log(LogLevel.Critical, new EventId(), new StructuredLogState(messageTemplate, args), exception, StructuredLogState.Formatter);
    }

    /// <summary>
    /// Begins a structured logging scope with properties.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="properties">The properties to include in the scope.</param>
    /// <returns>A disposable scope object.</returns>
    public static IDisposable? BeginStructuredScope(this ILogger logger, IDictionary<string, object?> properties)
    {
        return logger.BeginScope(properties);
    }

    /// <summary>
    /// Begins a structured logging scope with a single property.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="propertyValue">The value of the property.</param>
    /// <returns>A disposable scope object.</returns>
    public static IDisposable? BeginStructuredScope(this ILogger logger, string propertyName, object? propertyValue)
    {
        var properties = new Dictionary<string, object?> { { propertyName, propertyValue } };
        return logger.BeginScope(properties);
    }

    /// <summary>
    /// Begins a structured logging scope with multiple properties.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="messageTemplate">The message template for the scope.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    /// <returns>A disposable scope object.</returns>
    public static IDisposable? BeginStructuredScope(this ILogger logger, string messageTemplate, params object?[] args)
    {
        var parseResult = MessageTemplateParser.Parse(messageTemplate, args);
        return logger.BeginScope(parseResult.Properties);
    }
}