namespace pawKit.Core.IO;

/// <summary>
/// Defines the type of path separator to use in path operations.
/// </summary>
public enum PathSeparatorType
{
    /// <summary>
    /// Uses the default path separator for the current environment.
    /// </summary>
    Default,

    /// <summary>
    /// Uses the forward slash (/) as the path separator.
    /// </summary>
    ForwardSlash,

    /// <summary>
    /// Uses the backslash (\) as the path separator.
    /// </summary>
    Backslash,
}
