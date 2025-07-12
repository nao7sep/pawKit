namespace pawKitLib.Ai.Content;

/// <summary>
/// Specifies the location type of a resource referenced in a message part.
/// </summary>
public enum ResourceKind
{
    /// <summary>The resource is located at a remote URL.</summary>
    RemoteUrl,
    /// <summary>The resource is located at a local file system path.</summary>
    LocalPath,
    /// <summary>The resource is identified by a file ID specific to the AI provider.</summary>
    ProviderFileId,
    /// <summary>The resource content is provided directly as a Base64-encoded string.</summary>
    InlineBase64
}