using System.Text.Json.Serialization;

namespace pawKitLib.Ai.Providers.OpenAI.Dto;

/// <summary>
/// Represents the log probability information for a single token.
/// </summary>
internal sealed record OpenAiLogProbContent
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

    /// <summary>
    /// List of the most likely tokens and their log probability, at this token position.
    /// In rare cases, there may be fewer tokens than `top_logprobs`.
    /// </summary>
    [JsonPropertyName("top_logprobs")]
    public required IReadOnlyList<OpenAiTopLogProb> TopLogProbs { get; init; }
}