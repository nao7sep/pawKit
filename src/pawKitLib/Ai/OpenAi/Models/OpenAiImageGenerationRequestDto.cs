using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

public class OpenAiImageGenerationRequestDto : DynamicDto
{
    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("n")]
    public int N { get; set; } = 1;

    [JsonPropertyName("size")]
    public string Size { get; set; } = "1024x1024";

    // Optional: 'url' or 'b64_json'
    [JsonPropertyName("response_format")]
    public string ResponseFormat { get; set; } = "url";
}