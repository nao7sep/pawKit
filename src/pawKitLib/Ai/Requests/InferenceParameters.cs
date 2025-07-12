using System.Collections.Immutable;
using pawKitLib.Ai.Sessions;
using pawKitLib.Ai.Tools;

namespace pawKitLib.Ai.Requests;

/// <summary>
/// Defines provider-specific parameters for an AI inference request.
/// These are not part of the session history.
/// </summary>
/// <param name="Temperature">Controls randomness. Lower is more deterministic.</param>
/// <param name="MaxTokens">The maximum number of tokens to generate.</param>
/// <param name="TopP">Nucleus sampling parameter.</param>
/// <param name="StopSequences">A list of sequences that will stop generation.</param>
/// <param name="UserId">An identifier for the end-user, for monitoring and abuse detection.</param>
/// <param name="SystemPromptOverride">
/// If provided, this text will replace the system prompt for this request only. This allows for tactical, per-turn
/// instructions without modifying the canonical system message in the <see cref="AiSession.Messages"/> log.
/// </param>
/// <param name="ToolChoice">A specific constraint on how the model should use tools for this request.</param>
/// <param name="ModelId">The specific model ID to use for this request (e.g., "gpt-4o").</param>
/// <param name="ResponseFormat">A constraint on the format of the model's response (e.g., force JSON output).</param>
/// <param name="FrequencyPenalty">Penalizes new tokens based on their existing frequency in the text so far.</param>
/// <param name="PresencePenalty">Penalizes new tokens based on whether they appear in the text so far.</param>
/// <param name="Seed">An integer seed for deterministic, repeatable outputs.</param>
/// <param name="LogitBias">A map to modify the probability of specific tokens appearing in the completion. Use an immutable dictionary for true immutability.</param>
/// <param name="N">
/// How many chat completion choices to generate for each input message.
/// Corresponds to the 'n' parameter in the OpenAI API.
/// Defaults to 1 if not specified by the provider.
/// </param>
/// <param name="LogProbs">
/// Whether to return log probabilities of output tokens. If true, each output token will include the log probability.
/// </param>
/// <param name="TopLogProbs">
/// An integer between 0 and 5 specifying the number of most likely tokens to return at each token position, each with log probabilities.
/// Requires <see cref="LogProbs"/> to be true.
/// </param>
public sealed record InferenceParameters(
    float? Temperature = null,
    int? MaxTokens = null,
    float? TopP = null,
    ImmutableList<string>? StopSequences = null,
    string? UserId = null,
    string? SystemPromptOverride = null,
    ToolChoice? ToolChoice = null,
    string? ModelId = null,
    ResponseFormat? ResponseFormat = null,
    float? FrequencyPenalty = null,
    float? PresencePenalty = null,
    int? Seed = null,
    ImmutableDictionary<string, int>? LogitBias = null,
    int? N = null,
    bool? LogProbs = null,
    int? TopLogProbs = null);