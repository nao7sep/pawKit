namespace pawKitLib.Ai.Content;

/// <summary>
/// Specifies the type of content in a message part.
/// </summary>
public enum Modality
{
    /// <summary>Textual content.</summary>
    Text,
    /// <summary>Image content.</summary>
    Image,
    /// <summary>A request from the model to call one or more tools.</summary>
    ToolCall,
    /// <summary>The result of a tool execution.</summary>
    ToolResult,
    /// <summary>Audio content.</summary>
    Audio,
    /// <summary>Video content.</summary>
    Video,
    /// <summary>JSON content.</summary>
    Json,
    /// <summary>A document, such as a PDF or text file, where the specific format is indicated by the resource's MIME type.</summary>
    Document
}