using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Represents only the error details we know we'll receive from the OpenAI API.
/// </summary>
public class OpenAiErrorDto : BaseDto
{
    public string? Type { get; set; }       // e.g., "invalid_request_error"
    public string? Code { get; set; }       // e.g., "invalid_api_key"
    public string? Message { get; set; }    // Human-readable error message
    public string? Param { get; set; }      // Parameter related to the error, if any
}
