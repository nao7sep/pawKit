using pawKitLib.Models;
using System.Text.Json.Serialization;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Response DTO for OpenAI audio transcription. Only includes the transcribed text for simplicity.
/// Expand with more properties or use ExtraProperties if you need additional response data.
/// </summary>
public class OpenAiAudioTranscribeResponseDto : DynamicDto
{
    /// <summary>
    /// Transcribed text from the audio. Populated for basic 'json' and 'text' formats.
    /// </summary>
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}
