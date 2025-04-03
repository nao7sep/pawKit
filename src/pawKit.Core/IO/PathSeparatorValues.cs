using pawKit.Core.Platform;

namespace pawKit.Core.IO;

/// <summary>
/// Provides actual separator values for the corresponding <see cref="PathSeparatorType"/> values.
/// </summary>
public static class PathSeparatorValues
{
    /// <summary>
    /// Gets the default path separator for the current environment.
    /// </summary>
    public static readonly char Default = OperatingSystemInfo.DirectorySeparator;

    /// <summary>
    /// Gets the forward slash (/) path separator.
    /// </summary>
    public static readonly char ForwardSlash = '/';

    /// <summary>
    /// Gets the backslash (\) path separator.
    /// </summary>
    public static readonly char Backslash = '\\';

    /// <summary>
    /// Gets the separator character for the specified separator type.
    /// </summary>
    /// <param name="separatorType">The type of separator to get.</param>
    /// <returns>The character representing the specified separator type.</returns>
    public static char GetSeparator (PathSeparatorType separatorType) => separatorType switch
    {
        PathSeparatorType.ForwardSlash => ForwardSlash,
        PathSeparatorType.Backslash => Backslash,
        _ => Default
    };
}
