﻿﻿using System.Collections.Immutable;
using pawKitLib.Ai;
using pawKitLib.Ai.Content;

namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Represents a single message in an AI session, which can contain multiple content parts.
/// </summary>
public sealed record AiMessage
{
    /// <summary>A unique identifier for the message.</summary>
    public required Guid Id { get; init; }

    /// <summary>The role of the entity that generated this message.</summary>
    public required MessageRole Role { get; init; }

    /// <summary>The UTC timestamp when the message was created.</summary>
    public required DateTimeOffset CreatedAtUtc { get; init; }

    /// <summary>The collection of content parts that make up this message.</summary>
    public ImmutableList<IContentPart> Parts { get; init; } = ImmutableList<IContentPart>.Empty;
}