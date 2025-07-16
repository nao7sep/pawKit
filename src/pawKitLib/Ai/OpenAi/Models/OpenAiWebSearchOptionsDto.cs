using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Web search options for chat completion requests.
/// </summary>
public class OpenAiWebSearchOptionsDto : DynamicDto
{
    [JsonPropertyName("search_context_size")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? SearchContextSize { get; set; }

    [JsonPropertyName("user_location")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiUserLocationDto? UserLocation { get; set; }
}