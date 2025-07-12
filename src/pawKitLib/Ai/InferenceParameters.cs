﻿using System.Collections.Immutable;
using pawKitLib.Ai.Sessions;
using pawKitLib.Ai.Tools;

namespace pawKitLib.Ai;

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
    ImmutableDictionary<string, int>? LogitBias = null);