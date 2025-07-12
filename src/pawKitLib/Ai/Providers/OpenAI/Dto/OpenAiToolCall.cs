using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI.Dto;

/// <summary>
/// Represents a tool call requested by the model in an OpenAI API response.
/// </summary>
internal sealed record OpenAiToolCall
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("type")]
    public string Type { get; } = OpenAiApiConstants.ToolTypeFunction;

    [JsonPropertyName("function")]
    public required OpenAiToolCallFunction Function { get; init; }
}
