using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Response DTO for listing files from OpenAI.
/// </summary>
public class OpenAiFileListResponseDto : DynamicDto
{
    /// <summary>
    /// The object type, which is always "list".
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    /// <summary>
    /// List of file objects.
    /// </summary>
    [JsonPropertyName("data")]
    public List<OpenAiFileDto> Data { get; set; } = [];

    /// <summary>
    /// Whether there are more results available.
    /// </summary>
    [JsonPropertyName("has_more")]
    public bool HasMore { get; set; }
}