using pawKitLib.Models;

namespace pawKitLib.Ai.Models;

public class OpenAiConfigDto : BaseDto, IAiServiceConfigDto
{
    public string Provider { get; set; } = "OpenAI";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    // Add OpenAI-specific properties here if needed
}
