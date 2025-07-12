using System.Collections.Immutable;
using pawKitLib.Ai.Tools;

namespace pawKitLib.Ai.Content;

/// <summary>
/// Represents a content part containing one or more tool call requests from the model.
/// </summary>
/// <param name="ToolCalls">The collection of tool calls requested by the model.</param>
public sealed record ToolCallContentPart(ImmutableList<ToolCall> ToolCalls) : IContentPart
{
    public Modality Modality => Modality.ToolCall;
}