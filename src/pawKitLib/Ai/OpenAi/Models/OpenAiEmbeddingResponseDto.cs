using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

public class OpenAiEmbeddingResponseDto : DynamicDto
{
    [JsonPropertyName("object")]
    public string Object { get; set; } = "list";

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<OpenAiEmbeddingDto> Data { get; set; } = [];

    [JsonPropertyName("usage")]
    public OpenAiUsageDto Usage { get; set; } = new();
}
