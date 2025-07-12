namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Signals the end of the entire stream.
/// </summary>
/// <param name="FinishReason">The reason the stream was terminated (e.g., "stop", "tool_calls").</param>
public sealed record StreamEndPart(string FinishReason) : StreamingPart;