using pawKitLib.Models;

namespace pawKitLib.Ai.Xai;

public class XaiConfigDto : BaseDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
