using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Breakdown of tokens used in a completion.
/// </summary>
public class OpenAiCompletionTokensDetailsDto : DynamicDto
{
    [JsonPropertyName("accepted_prediction_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AcceptedPredictionTokens { get; set; }

    [JsonPropertyName("audio_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AudioTokens { get; set; }

    [JsonPropertyName("reasoning_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ReasoningTokens { get; set; }

    [JsonPropertyName("rejected_prediction_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? RejectedPredictionTokens { get; set; }
}