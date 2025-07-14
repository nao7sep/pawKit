using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

public class OpenAiConfigDto : DynamicDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}
