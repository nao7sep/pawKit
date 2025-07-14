using pawKitLib.Models;

namespace pawKitLib.Ai.Models;

public class XaiConfigDto : BaseDto, IAiServiceConfigDto
{
    public string Provider { get; set; } = "xAI";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    // Add xAI-specific properties here if needed
}
