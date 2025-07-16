using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Approximate location parameters for web search.
/// </summary>
public class OpenAiApproximateLocationDto : DynamicDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "approximate";
}