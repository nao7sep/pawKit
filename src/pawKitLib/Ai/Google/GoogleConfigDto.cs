using pawKitLib.Models;

namespace pawKitLib.Ai.Google;

public class GoogleConfigDto : BaseDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
