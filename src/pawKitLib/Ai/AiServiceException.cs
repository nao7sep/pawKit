using System;

namespace pawKitLib.Ai;

/// <summary>
/// Represents errors returned from AI providers, including all available server response details.
/// </summary>
public class AiServiceException : Exception
{
    /// <summary>
    /// The status code returned by the provider, if available (may be null).
    /// </summary>
    public string? StatusCode { get; }
    /// <summary>
    /// The raw response payload from the provider, if available (may be null).
    /// </summary>
    public string? RawResponse { get; }
    /// <summary>
    /// Provider-specific error details, such as a DTO (e.g., OpenAiErrorDto).
    /// </summary>
    public object? ProviderDetails { get; }

    public AiServiceException() { }

    public AiServiceException(string message) : base(message) { }

    public AiServiceException(string message, Exception innerException) : base(message, innerException) { }

    public AiServiceException(string message, string? statusCode = null, string? rawResponse = null, object? providerDetails = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        RawResponse = rawResponse;
        ProviderDetails = providerDetails;
    }
}
