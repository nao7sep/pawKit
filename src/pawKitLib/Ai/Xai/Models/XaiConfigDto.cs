using pawKitLib.Models;

namespace pawKitLib.Ai.Xai.Models;

public class XaiConfigDto : DynamicDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
