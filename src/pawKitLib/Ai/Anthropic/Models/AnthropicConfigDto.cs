using pawKitLib.Models;

namespace pawKitLib.Ai.Anthropic.Models;

public class AnthropicConfigDto
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    /// <summary>
    /// The required 'anthropic-version' header value (e.g., "2023-06-01").
    ///
    /// This property specifies the version of the Anthropic API being targeted, not the model version.
    /// API version is a system-wide configuration and should not be adjusted by end users on a per-request basis.
    /// It is essential for all requests to be accepted by Anthropic's API and governs compatibility and feature set for the entire application.
    /// In contrast, model selection is typically a user-facing parameter, while API version is a foundational setting.
    /// </summary>
    public string ApiVersion { get; set; } = string.Empty;
}
