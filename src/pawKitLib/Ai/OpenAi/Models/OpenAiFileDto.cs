using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Represents a file reference for multi-modal chat completion.
/// </summary>
public class OpenAiFileDto : DynamicDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}
