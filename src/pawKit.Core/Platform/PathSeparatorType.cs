namespace pawKit.Core.Platform;

/// <summary>
/// Represents the type of path separator character used to separate paths in environment variables.
/// </summary>
/// <remarks>
/// A path separator is different from a directory separator:
/// - A directory separator (like '\' or '/') is used within a single file path to separate directories.
/// - A path separator (like ';' or ':') is used to separate multiple complete file paths in environment
///   variables like PATH, where each path points to a different location.
/// </remarks>
public enum PathSeparatorType
{
    Default,
    Windows,
    UnixLike
}