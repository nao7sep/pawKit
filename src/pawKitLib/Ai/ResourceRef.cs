namespace pawKitLib.Ai;

/// <summary>
/// Represents a reference to a resource, such as an image or video file.
/// </summary>
/// <param name="Kind">The kind of resource location (e.g., URL, local path).</param>
/// <param name="Value">The value of the resource location (e.g., the URL string, file path, provider file ID, or Base64-encoded content).</param>
/// <param name="MimeType">The optional MIME type of the resource.</param>
/// <param name="Length">The optional length of the resource in bytes.</param>
public sealed record ResourceRef(ResourceKind Kind, string Value, string? MimeType = null, long? Length = null);