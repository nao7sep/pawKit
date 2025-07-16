using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// User location parameters for web search.
/// </summary>
public class OpenAiUserLocationDto : DynamicDto
{
    [JsonPropertyName("approximate")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiApproximateLocationDto? Approximate { get; set; }
}