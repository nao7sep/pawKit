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

    /// <summary>The ordered list of messages exchanged during the session.</summary>
    public ImmutableList<AiMessage> Messages { get; init; } = ImmutableList<AiMessage>.Empty;

    /// <summary>The set of tools available for the model to call during the session.</summary>
    public ImmutableList<ToolDefinition> AvailableTools { get; init; } = ImmutableList<ToolDefinition>.Empty;

    /// <summary>A collection of key-value pairs for storing application-specific metadata.</summary>
    public ImmutableDictionary<string, string> Metadata { get; init; } = ImmutableDictionary<string, string>.Empty;

    /// <summary>A map of message-specific overrides for context construction.</summary>
    public ImmutableDictionary<Guid, MessageContextOverride> ContextOverrides { get; init; } = ImmutableDictionary<Guid, MessageContextOverride>.Empty;
}