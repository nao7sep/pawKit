namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Signals the start of a tool call from the stream.
/// </summary>
/// <param name="ToolCallId">The unique identifier for this tool call.</param>
/// <param name="FunctionName">The name of the function being called.</param>
public sealed record ToolCallStartStreamPart(string ToolCallId, string FunctionName) : StreamingPart;