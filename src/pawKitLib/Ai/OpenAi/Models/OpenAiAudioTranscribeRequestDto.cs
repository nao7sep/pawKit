using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Request DTO for OpenAI audio transcription. Only includes the required file and model for simplicity.
/// Add more properties as needed; use ExtraProperties for experimental or optional parameters.
/// </summary>
public class OpenAiAudioTranscribeRequestDto : DynamicDto
{
    /// <summary>
    /// Audio file to transcribe.
    /// Accepts FilePathReferenceDto (local path) or FileContentDto (in-memory bytes).
    /// Consumers must handle the actual type appropriately.
    /// </summary>
    [DtoOutputIgnore]
    public object File { get; set; } = new();

    /// <summary>
    /// Model ID to use for transcription.
    ///
    /// Note: This property does not fall back to a configuration value such as OpenAiConfigDto.TranscriptionModel.
    /// The model parameter is intended to be explicitly set by the user, rather than treated as a system-wide configuration.
    /// While some systems use configuration for model selection, this approach maintains user control and avoids hidden defaults.
    ///
    /// Implementing fallback logic would add complexity, as MultipartFormDataContent does not support direct updates or deletions of existing parts.
    /// Determining whether to overwrite a value would require inspecting the content before adding it, likely necessitating additional parameters and logic.
    ///
    /// For clarity and maintainability, this DTO prioritizes simplicity and explicitness over implicit fallback behavior.
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("language")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Language { get; set; }

    [JsonPropertyName("prompt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Prompt { get; set; }

    [JsonPropertyName("response_format")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ResponseFormat { get; set; }

    [JsonPropertyName("temperature")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? Temperature { get; set; }

    [JsonPropertyName("timestamp_granularities")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<string>? TimestampGranularities { get; set; }
}
