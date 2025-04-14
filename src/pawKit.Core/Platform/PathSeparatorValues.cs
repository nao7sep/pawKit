namespace pawKit.Core.Platform;

using System;

public static class PathSeparatorValues
{
    public const char WindowsPathSeparator = ';';

    public const char UnixLikePathSeparator = ':';

    private static readonly Lazy<char> _defaultPathSeparator = new(() =>
        OperatingSystemInfo.IsWindows ? WindowsPathSeparator : UnixLikePathSeparator);

    public static char DefaultPathSeparator => _defaultPathSeparator.Value;

    public static char GetPathSeparator(PathSeparatorType type) => type switch
    {
        PathSeparatorType.Windows => WindowsPathSeparator,
        PathSeparatorType.UnixLike => UnixLikePathSeparator,
        _ => DefaultPathSeparator
    };
}