using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

public class OpenAiEmbeddingRequestDto : DynamicDto
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "text-embedding-ada-002";

    [JsonPropertyName("input")]
    public object Input { get; set; } = new(); // Can be string or list of strings
}