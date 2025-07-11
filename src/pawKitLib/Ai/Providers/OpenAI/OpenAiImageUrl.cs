using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents the URL structure for an image in the OpenAI API.
/// </summary>
internal sealed record OpenAiImageUrl([property: JsonPropertyName("url")] string Url);