using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Represents only the error details we know we'll receive from the OpenAI API.
/// </summary>
public class OpenAiErrorDto : DynamicDto
{
    /// <summary>
    /// The error type, e.g., "invalid_request_error".
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// The error code, e.g., "invalid_api_key".
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Parameter related to the error, if any.
    /// </summary>
    public string? Param { get; set; }
}
