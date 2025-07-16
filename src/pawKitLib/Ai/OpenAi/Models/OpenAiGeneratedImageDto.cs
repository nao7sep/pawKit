using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

public class OpenAiGeneratedImageDto : DynamicDto
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("b64_json")]
    public string? B64Json { get; set; }
}
