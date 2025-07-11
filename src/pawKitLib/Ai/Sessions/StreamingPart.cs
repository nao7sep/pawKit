namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Represents a part of a streamed AI response. This is a sealed base record for a discriminated union.
/// </summary>
public abstract record StreamingPart;

/// <summary>
/// Represents a chunk of text content from the stream.
/// </summary>
/// <param name="Text">The text content delta.</param>
public sealed record TextStreamPart(string Text) : StreamingPart;

/// <summary>
/// Signals the start of a tool call from the stream.
/// </summary>
/// <param name="ToolCallId">The unique identifier for this tool call.</param>
/// <param name="FunctionName">The name of the function being called.</param>
public sealed record ToolCallStartStreamPart(string ToolCallId, string FunctionName) : StreamingPart;

/// <summary>
/// Represents a chunk of the arguments for a tool call from the stream.
/// </summary>
/// <param name="ToolCallId">The ID of the tool call this chunk belongs to.</param>
/// <param name="ArgumentChunk">The partial JSON string for the arguments.</param>
public sealed record ToolCallArgumentStreamPart(string ToolCallId, string ArgumentChunk) : StreamingPart;

/// <summary>
/// Signals the end of the entire stream.
/// </summary>
/// <param name="FinishReason">The reason the stream was terminated (e.g., "stop", "tool_calls").</param>
public sealed record StreamEndPart(string FinishReason) : StreamingPart;