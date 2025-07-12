using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI.Dto;

/// <summary>
/// Represents the log probability information for the choice.
/// </summary>
internal sealed record OpenAiLogProbs
{
    /// <summary>
    /// A list of message content tokens with log probability information.
    /// </summary>
    [JsonPropertyName("content")]
    public IReadOnlyList<OpenAiLogProbContent>? Content { get; init; }
}