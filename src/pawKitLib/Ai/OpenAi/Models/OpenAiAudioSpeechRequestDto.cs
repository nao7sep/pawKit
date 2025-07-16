using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

public class OpenAiAudioSpeechRequestDto : DynamicDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("voice")]
    public string Voice { get; set; } = string.Empty;

    // Optional: 'text' or 'json'
    [JsonPropertyName("response_format")]
    public string ResponseFormat { get; set; } = string.Empty;
}
