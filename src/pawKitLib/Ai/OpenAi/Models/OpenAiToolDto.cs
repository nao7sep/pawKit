using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Represents a tool (function) that the model can call.
/// </summary>
public class OpenAiToolDto : DynamicDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("function")]
    public OpenAiFunctionDto Function { get; set; } = new();
}
