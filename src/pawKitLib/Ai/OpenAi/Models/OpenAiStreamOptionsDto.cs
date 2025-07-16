using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Options for streaming response.
/// </summary>
public class OpenAiStreamOptionsDto : DynamicDto
{
    [JsonPropertyName("include_usage")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? IncludeUsage { get; set; }
}