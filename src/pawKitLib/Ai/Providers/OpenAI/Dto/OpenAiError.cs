using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI.Dto;

/// <summary>
/// Represents the detailed error object from the OpenAI API.
/// </summary>
internal sealed record OpenAiError
{
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }

    [JsonPropertyName("param")]
    public string? Param { get; init; }

    [JsonPropertyName("type")]
    public required string Type { get; init; }
}
