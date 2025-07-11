namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents an exception thrown when the OpenAI API returns a non-success status code.
/// </summary>
public sealed class OpenAiApiException : Exception
{
    /// <summary>The HTTP status code returned by the API.</summary>
    public int StatusCode { get; }

    /// <summary>The error type returned by the API (e.g., "invalid_request_error").</summary>
    public string? ErrorType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAiApiException"/> class.
    /// </summary>
    public OpenAiApiException(string message, int statusCode, string? errorType, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        ErrorType = errorType;
    }
}