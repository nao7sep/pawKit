using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

public class OpenAiConfigDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;

    // This config does not include a property for the transcription model.
    // Model selection is intentionally left to the request DTO, not global configuration.
    // For the rationale behind this design, see the comment on the Model property in OpenAiAudioTranscribeRequestDto.
}
