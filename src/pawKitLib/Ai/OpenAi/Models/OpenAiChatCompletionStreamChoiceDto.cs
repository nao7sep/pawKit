using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Represents a single choice in a streaming chat completion chunk.
/// For streaming, we get deltas instead of complete messages.
/// </summary>
public class OpenAiChatCompletionStreamChoiceDto : DynamicDto
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("delta")]
    public OpenAiChatMessageDto? Delta { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("logprobs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiLogprobsDto? Logprobs { get; set; }
}