using System.Collections.Immutable;

namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Defines provider-specific parameters for an AI inference request.
/// These are not part of the session history.
/// </summary>
/// <param name="Temperature">Controls randomness. Lower is more deterministic.</param>
/// <param name="MaxTokens">The maximum number of tokens to generate.</param>
/// <param name="TopP">Nucleus sampling parameter.</param>
/// <param name="StopSequences">A list of sequences that will stop generation.</param>
/// <param name="UserId">An identifier for the end-user, for monitoring and abuse detection.</param>
/// <param name="SystemPromptOverride">If provided, this text will replace the system prompt for this request only.</param>
/// <param name="ToolChoice">A specific constraint on how the model should use tools for this request.</param>
/// <param name="ModelId">The specific model ID to use for this request (e.g., "gpt-4o").</param>
/// <param name="ResponseFormat">A constraint on the format of the model's response (e.g., force JSON output).</param>
public sealed record InferenceParameters(
    float? Temperature = null,
    int? MaxTokens = null,
    float? TopP = null,
    ImmutableList<string>? StopSequences = null,
    string? UserId = null,
    string? SystemPromptOverride = null,
    ToolChoice? ToolChoice = null,
    string? ModelId = null,
    ResponseFormat? ResponseFormat = null);