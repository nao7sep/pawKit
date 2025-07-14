namespace pawKitLib.Models;

/// <summary>
/// Strongly-typed thin wrapper for a remote file URL. Prefer this over using a plain string for clarity and type safety.
/// Enables easy refactoring to polymorphic types if local and remote files need to be unified in the future.
/// </summary
public class FileUrlReferenceDto : DynamicDto
{
    public string Url { get; set; } = string.Empty;
}
