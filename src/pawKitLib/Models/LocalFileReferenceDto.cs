namespace pawKitLib.Models;

/// <summary>
/// Strongly-typed thin wrapper for a local file path. Prefer this over using a plain string for clarity and type safety.
/// If requirements change, this can be refactored to support remote files or polymorphic types easily.
/// </summary
public class LocalFileReferenceDto
{
    public string FilePath { get; set; } = string.Empty;
}
