using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Breakdown of tokens used in the prompt.
/// </summary>
public class OpenAiPromptTokensDetailsDto : DynamicDto
{
    [JsonPropertyName("audio_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? AudioTokens { get; set; }

    [JsonPropertyName("cached_tokens")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? CachedTokens { get; set; }
}
