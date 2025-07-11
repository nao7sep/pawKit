using System.Collections.Immutable;

namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Represents the final, prepared context to be sent to an AI provider.
/// This is the output of the <see cref="Abstractions.IRequestContextBuilder"/>.
/// </summary>
/// <param name="Messages">The final, filtered, and resolved list of messages.</param>
/// <param name="SystemPrompt">The final system prompt, after applying any overrides.</param>
/// <param name="AvailableTools">The list of available tools for the request.</param>
public sealed record AiRequestContext(
    ImmutableList<AiMessage> Messages,
    string? SystemPrompt,
    ImmutableList<ToolDefinition> AvailableTools);