namespace pawKitLib.Ai.Content;

/// <summary>
/// Represents the result of a tool execution, to be sent back to the model.
/// </summary>
/// <param name="ToolCallId">The ID of the tool call this result corresponds to.</param>
/// <param name="Content">The output or result from the tool execution.</param>
public sealed record ToolResultContentPart(string ToolCallId, string Content) : IContentPart
{
    public Modality Modality => Modality.ToolResult;
}