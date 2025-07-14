namespace pawKitLib.Models;

/// <summary>
/// Strongly-typed wrapper for in-memory file content, including file name and bytes.
/// Use this for scenarios where file data is loaded into memory, such as uploads or API requests.
/// </summary>
/// <remarks>
/// Note: This class does not require the features provided by DynamicDto, as it is intended solely for strongly-typed in-memory file content representation.
/// </remarks>
public class FileContentDto
{
    /// <summary>
    /// The name of the file, including extension.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The file content as a byte array. Use this property to hold the actual file data in memory.
    /// </summary>
    public byte[] Bytes { get; set; } = [];
}
