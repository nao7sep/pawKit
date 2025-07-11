namespace pawKitLib.Ai;

/// <summary>
/// Represents a non-textual, media-based part of a multi-modal message, such as an image.
/// </summary>
/// <param name="Modality">The modality of the content, e.g., Image.</param>
/// <param name="Resource">A reference to the media resource.</param>
public sealed record MediaContentPart(Modality Modality, ResourceRef Resource) : IContentPart;