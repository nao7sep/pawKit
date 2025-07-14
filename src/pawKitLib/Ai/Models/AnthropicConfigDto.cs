using pawKitLib.Models;

namespace pawKitLib.Ai.Models;

public class AnthropicConfigDto : BaseDto, IAiServiceConfigDto
{
    public string Provider { get; set; } = "Anthropic";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    // Add Anthropic-specific properties here if needed
}
