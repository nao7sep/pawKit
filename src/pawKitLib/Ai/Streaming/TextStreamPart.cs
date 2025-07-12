namespace pawKitLib.Ai.Streaming;

/// <summary>
/// Represents a chunk of text content from the stream.
/// </summary>
/// <param name="Text">The text content delta.</param>
public sealed record TextStreamPart(string Text) : StreamingPart;