using pawKit.Core.Platform;

namespace pawKit.Core.IO;

public static class PathSeparatorValues
{
    public static readonly char Default = OperatingSystemInfo.DirectorySeparator;

    public static readonly char ForwardSlash = '/';

    public static readonly char Backslash = '\\';

    public static char GetSeparator(PathSeparatorType separatorType) => separatorType switch
    {
        PathSeparatorType.ForwardSlash => ForwardSlash,
        PathSeparatorType.Backslash => Backslash,
        _ => Default
    };
}