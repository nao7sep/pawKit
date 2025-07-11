using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents a tool definition for the OpenAI API.
/// </summary>
internal sealed record OpenAiTool
{
    [JsonPropertyName("type")]
    public string Type { get; } = "function";

    [JsonPropertyName("function")]
    public required OpenAiFunctionDefinition Function { get; init; }
}