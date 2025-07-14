using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Represents the top-level error object from the OpenAI API, which contains a nested "error" property.
/// </summary>
public class OpenAiErrorContainerDto : DynamicDto
{
    [JsonPropertyName("error")]
    public OpenAiErrorDto? Error { get; set; }
}
