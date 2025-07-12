using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents an image content part for the OpenAI API.
/// </summary>
internal sealed record OpenAiImageContentPart
{
    [JsonPropertyName("type")]
    public string Type { get; } = OpenAiApiConstants.ContentTypeImageUrl;

    [JsonPropertyName("image_url")]
    public required OpenAiImageUrl ImageUrl { get; init; }
}