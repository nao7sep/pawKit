using pawKitLib.Models;

namespace pawKitLib.Ai.Anthropic.Models;

public class AnthropicConfigDto : DynamicDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    // This should contain the required 'anthropic-version' header value (e.g., "2023-06-01").
    // It's not the model version — it's the version of the Anthropic API being targeted.
    // This is necessary for all requests to Anthropic's API to be accepted.
    public string ApiVersion { get; set; } = string.Empty;
}
