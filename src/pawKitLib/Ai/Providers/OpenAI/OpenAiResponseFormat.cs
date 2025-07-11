using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents the response_format parameter for the OpenAI API.
/// </summary>
internal sealed record OpenAiResponseFormat
{
    [JsonPropertyName("type")]
    public required string Type { get; init; }
}