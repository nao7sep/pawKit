using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents the function name within an OpenAI tool choice object.
/// </summary>
internal sealed record OpenAiToolChoiceFunction
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}