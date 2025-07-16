using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

public class OpenAiGeneratedImageDto : DynamicDto
{
    [JsonPropertyName("url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Url { get; set; }

    [JsonPropertyName("b64_json")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? B64Json { get; set; }

    [JsonPropertyName("revised_prompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RevisedPrompt { get; set; }
}
