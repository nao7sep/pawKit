namespace pawKitLib.Models;

/// <summary>
/// Strongly-typed thin wrapper for a remote file URL. Prefer this over using a plain string for clarity and type safety.
/// Enables easy refactoring to polymorphic types if local and remote files need to be unified in the future.
/// </summary
/// <remarks>
/// Note: This class does not require the features provided by DynamicDto, as it is intended solely for strongly-typed file URL representation.
/// </remarks>
public class FileUrlReferenceDto
{
    public string Url { get; set; } = string.Empty;
}
