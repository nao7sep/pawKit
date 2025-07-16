using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Represents a file object from OpenAI, used for file management and multi-modal chat completion.
/// </summary>
public class OpenAiFileDto : DynamicDto
{
    /// <summary>
    /// The file identifier, which can be referenced in API endpoints.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The object type, which is always "file".
    /// </summary>
    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    /// <summary>
    /// The size of the file, in bytes.
    /// </summary>
    [JsonPropertyName("bytes")]
    public int Bytes { get; set; }

    /// <summary>
    /// The Unix timestamp (in seconds) for when the file was created.
    /// </summary>
    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    [JsonPropertyName("filename")]
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// The intended purpose of the file. Possible values are "assistants", "vision", "batch", and "fine-tune".
    /// </summary>
    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Deprecated. The current status of the file, which can be either "uploaded", "processed", or "error".
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    /// <summary>
    /// Deprecated. For details on why a fine-tuning training file failed validation, see the error field on fine-tuning jobs.
    /// </summary>
    [JsonPropertyName("status_details")]
    public string? StatusDetails { get; set; }
}
