using pawKitLib.Models;

namespace pawKitLib.Ai.Models;

public class GoogleConfigDto : BaseDto, IAiServiceConfigDto
{
    public string Provider { get; set; } = "Google";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    // Add Google-specific properties here if needed
}
