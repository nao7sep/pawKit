using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Request DTO for uploading files to OpenAI.
/// Supports both file path references and in-memory file content.
/// </summary>
public class OpenAiFileUploadRequestDto : DynamicDto
{
    /// <summary>
    /// The file to upload. Can be FilePathReferenceDto or FileContentDto.
    /// </summary>
    [DtoOutputIgnore]
    public object File { get; set; } = null!;

    /// <summary>
    /// The intended purpose of the uploaded file.
    /// Use "assistants" for Assistants and Messages, "vision" for Assistants image inputs,
    /// "batch" for Batch API, and "fine-tune" for fine-tuning.
    /// </summary>
    [JsonPropertyName("purpose")]
    public string Purpose { get; set; } = string.Empty;
}
