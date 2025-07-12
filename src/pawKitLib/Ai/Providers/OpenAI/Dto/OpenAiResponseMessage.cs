using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI.Dto;

/// <summary>
/// Represents the message object within an OpenAI API response choice.
/// </summary>
internal sealed record OpenAiResponseMessage
{
    [JsonPropertyName("role")]
    public required string Role { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }

    [JsonPropertyName("tool_calls")]
    public IReadOnlyList<OpenAiToolCall>? ToolCalls { get; init; }
}
