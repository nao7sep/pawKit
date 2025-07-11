using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents the function details of a tool call in the OpenAI API.
/// </summary>
internal sealed record OpenAiToolCallFunction
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("arguments")]
    public required string Arguments { get; init; }
}