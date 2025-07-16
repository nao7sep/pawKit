using System.Collections.Generic;
using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Response from the OpenAI chat completion API.
/// Includes core identifiers and the list of choices, keeping it focused.
/// </summary>
public class OpenAiChatCompletionResponseDto : DynamicDto
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
    public List<OpenAiChatCompletionChoiceDto> Choices { get; set; } = new();

    [JsonPropertyName("usage")]
    public OpenAiUsageDto? Usage { get; set; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; set; }
}
