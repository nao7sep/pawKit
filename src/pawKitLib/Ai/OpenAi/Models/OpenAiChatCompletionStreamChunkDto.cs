using System.Collections.Generic;
using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Represents a single chunk in a streaming chat completion response.
/// Keeps it minimal - just the essentials for streaming.
/// </summary>
public class OpenAiChatCompletionStreamChunkDto : DynamicDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public long Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public List<OpenAiChatCompletionStreamChoiceDto> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public OpenAiUsageDto? Usage { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}
