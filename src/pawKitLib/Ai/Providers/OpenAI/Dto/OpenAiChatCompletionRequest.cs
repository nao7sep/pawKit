using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI.Dto;

/// <summary>
/// Represents the request payload for the OpenAI Chat Completions API.
/// This is an internal DTO and should not be exposed publicly.
/// </summary>
internal sealed record OpenAiChatCompletionRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("messages")]
    public required IReadOnlyList<OpenAiMessage> Messages { get; init; }

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? Temperature { get; init; }

    [JsonPropertyName("max_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("top_p")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? TopP { get; init; }

    [JsonPropertyName("stop")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<string>? Stop { get; init; }

    [JsonPropertyName("response_format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiResponseFormat? ResponseFormat { get; init; }

    [JsonPropertyName("tools")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<OpenAiTool>? Tools { get; init; }

    [JsonPropertyName("tool_choice")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? ToolChoice { get; init; }

    [JsonPropertyName("frequency_penalty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? FrequencyPenalty { get; init; }

    [JsonPropertyName("presence_penalty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? PresencePenalty { get; init; }

    [JsonPropertyName("seed")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Seed { get; init; }

    [JsonPropertyName("logit_bias")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, float>? LogitBias { get; init; }

    [JsonPropertyName("user")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? User { get; init; }
}
