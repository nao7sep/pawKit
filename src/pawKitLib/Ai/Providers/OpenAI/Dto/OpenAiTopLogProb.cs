using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI.Dto;

/// <summary>
/// Represents one of the most likely tokens and its log probability.
/// </summary>
internal sealed record OpenAiTopLogProb
{
    [JsonPropertyName("token")]
    public required string Token { get; init; }

    [JsonPropertyName("logprob")]
    public float LogProb { get; init; }

    /// <summary>
    /// A list of integers representing the UTF-8 bytes representation of the token.
    /// Useful in instances where characters are represented by multiple tokens and can be used to reconstruct the original character representation.
    /// Can be null if there is no byte representation for the token.
    /// </summary>
    [JsonPropertyName("bytes")]
    public IReadOnlyList<byte>? Bytes { get; init; }
}