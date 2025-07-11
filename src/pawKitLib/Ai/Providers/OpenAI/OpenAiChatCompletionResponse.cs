using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Represents the response payload from the OpenAI Chat Completions API.
/// </summary>
internal sealed record OpenAiChatCompletionResponse
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("object")]
    public required string Object { get; init; }

    [JsonPropertyName("created")]
    public long Created { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("choices")]
    public required IReadOnlyList<OpenAiChoice> Choices { get; init; }

    [JsonPropertyName("usage")]
    public required OpenAiUsage Usage { get; init; }
}