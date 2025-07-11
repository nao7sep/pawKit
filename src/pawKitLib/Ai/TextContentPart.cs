namespace pawKitLib.Ai;

/// <summary>
/// Represents a textual part of a multi-modal message.
/// </summary>
/// <param name="Text">The text content.</param>
public sealed record TextContentPart(string Text) : IContentPart
{
    public Modality Modality => Modality.Text;
}