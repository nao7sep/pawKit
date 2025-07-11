using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents a specific function choice for the OpenAI 'tool_choice' parameter.
/// </summary>
internal sealed record OpenAiToolChoice
{
    [JsonPropertyName("type")]
    public string Type { get; } = "function";

    [JsonPropertyName("function")]
    public required OpenAiToolChoiceFunction Function { get; init; }
}