using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents a text content part for the OpenAI API.
/// </summary>
internal sealed record OpenAiTextContentPart
{
    [JsonPropertyName("type")]
    public string Type { get; } = OpenAiApiConstants.ContentTypeText;

    [JsonPropertyName("text")]
    public required string Text { get; init; }
}