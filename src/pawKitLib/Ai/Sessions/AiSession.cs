﻿using System.Collections.Immutable;
namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Represents a complete, ordered sequence of messages in a conversation with an AI model.
/// This record is an immutable snapshot of a session's state.
/// </summary>
/// <remarks>
/// For guidance on using this immutable model in stateful UI applications (e.g., MVVM),
/// refer to the ARCHITECTURE.md document in the project's root.
/// </summary>
public sealed record AiSession
{
    /// <summary>A unique identifier for the session.</summary>
    public required Guid Id { get; init; }

    /// <summary>The UTC timestamp when the session was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>
    /// The canonical, immutable log of all messages exchanged during the session.
    /// This includes the initial <see cref="MessageRole.System"/> message that defines the baseline context.
    /// Note that this may not be the exact list sent to the provider; see <see cref="ContextOverrides"/> and <see cref="InferenceParameters.SystemPromptOverride"/>.
    /// </summary>
    public ImmutableList<AiMessage> Messages { get; init; } = ImmutableList<AiMessage>.Empty;

    /// <summary>
    /// The master set of tools available for the model to call during this session.
    /// This list is sent with every request unless constrained by <see cref="InferenceParameters.ToolChoice"/>.
    /// </summary>
    public ImmutableList<ToolDefinition> AvailableTools { get; init; } = ImmutableList<ToolDefinition>.Empty;

    /// <summary>A collection of key-value pairs for storing application-specific metadata.</summary>
    public ImmutableDictionary<string, string> Metadata { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>
    /// A map of message-specific overrides used by an <see cref="Abstractions.IRequestContextBuilder"/> to construct the final request payload.
    /// This is the primary mechanism for managing cost and token limits by excluding messages or replacing them with summaries without altering the canonical <see cref="Messages"/> log.
    /// </summary>
    public ImmutableDictionary<Guid, MessageContextOverride> ContextOverrides { get; init; } = ImmutableDictionary<Guid, MessageContextOverride>.Empty;
}