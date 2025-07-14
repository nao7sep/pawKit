using System.Text.Json.Serialization;
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
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// The error code, e.g., "invalid_api_key".
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; set; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Parameter related to the error, if any.
    /// </summary>
    [JsonPropertyName("param")]
    public string? Param { get; set; }
}
