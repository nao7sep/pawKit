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
    public object File { get; set; } = new();

    /// <summary>
    /// Model ID to use for transcription.
    /// </summary>
    public string Model { get; set; } = string.Empty;
}
