using System.Text.Json;
using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI.Dto;

/// <summary>
/// Represents the function definition within an OpenAI tool.
/// </summary>
internal sealed record OpenAiFunctionDefinition
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("parameters")]
    public required JsonElement Parameters { get; init; }
}
