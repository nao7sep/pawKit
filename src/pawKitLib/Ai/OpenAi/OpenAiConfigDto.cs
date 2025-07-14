using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi;

public class OpenAiConfigDto : BaseDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
