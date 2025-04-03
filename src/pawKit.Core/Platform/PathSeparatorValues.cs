namespace pawKit.Core.Platform;

/// <summary>
/// Provides methods for working with path separator characters.
/// </summary>
/// <remarks>
/// Path separators are used to separate multiple complete file paths in environment variables,
/// not to be confused with directory separators which separate directories within a single path.
/// </remarks>
public static class PathSeparatorValues
{
    public static char GetSeparator(PathSeparatorType separatorType) => separatorType switch
    {
        PathSeparatorType.Windows => OperatingSystemInfo.WindowsPathSeparator,
        PathSeparatorType.UnixLike => OperatingSystemInfo.UnixLikePathSeparator,
        _ => OperatingSystemInfo.CurrentPathSeparator
    };
}