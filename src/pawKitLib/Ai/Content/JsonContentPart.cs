namespace pawKitLib.Ai.Content;

/// <summary>
/// Represents a JSON object as part of a multi-modal message.
/// </summary>
/// <param name="JsonString">The JSON content, as a string.</param>
public sealed record JsonContentPart(string JsonString) : IContentPart
{
    public Modality Modality => Modality.Json;
}