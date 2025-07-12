namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Represents a chunk of the arguments for a tool call from the stream.
/// </summary>
/// <param name="ToolCallId">The ID of the tool call this chunk belongs to.</param>
/// <param name="ArgumentChunk">The partial JSON string for the arguments.</param>
public sealed record ToolCallArgumentStreamPart(string ToolCallId, string ArgumentChunk) : StreamingPart;