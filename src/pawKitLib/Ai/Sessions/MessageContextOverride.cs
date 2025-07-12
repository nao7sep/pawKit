﻿using System.Collections.Immutable;
using pawKitLib.Ai.Content;

namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Defines an override for a message's content when constructing a request context for an AI provider.
/// This allows for excluding messages or replacing them with summaries to manage cost and token limits.
/// </summary>
public sealed record MessageContextOverride
{
    /// <summary>If true, this message will be excluded from the context. Defaults to false.</summary>
    public bool IsExcluded { get; init; }

    /// <summary>If provided, these content parts will be used instead of the original message parts.</summary>
    public ImmutableList<IContentPart>? AlternateParts { get; init; }
}