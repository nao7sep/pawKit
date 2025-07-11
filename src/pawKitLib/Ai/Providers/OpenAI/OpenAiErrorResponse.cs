using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents the top-level error response payload from the OpenAI API.
/// </summary>
internal sealed record OpenAiErrorResponse
{
    [JsonPropertyName("error")]
    public required OpenAiError Error { get; init; }
}