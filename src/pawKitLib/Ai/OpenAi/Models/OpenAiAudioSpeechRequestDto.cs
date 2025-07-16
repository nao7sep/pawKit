using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

public class OpenAiAudioSpeechRequestDto : DynamicDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "tts-1";

    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("voice")]
    public string Voice { get; set; } = "alloy";

    // Optional: 'text' or 'json'
    [JsonPropertyName("response_format")]
    public string ResponseFormat { get; set; } = "mp3";
}