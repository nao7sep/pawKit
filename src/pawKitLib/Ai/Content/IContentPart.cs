namespace pawKitLib.Ai.Content;

/// <summary>
/// Defines a part of a multi-modal message content.
/// </summary>
public interface IContentPart
{
    /// <summary>Gets the modality of the content part.</summary>
    Modality Modality { get; }
}