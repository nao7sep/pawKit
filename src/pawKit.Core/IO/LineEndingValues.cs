using pawKit.Core.Platform;

namespace pawKit.Core.IO;

public static class LineEndingValues
{
    public static string GetLineEnding(LineEndingType lineEndingType) => lineEndingType switch
    {
        LineEndingType.Windows => OperatingSystemInfo.WindowsLineEnding,
        LineEndingType.UnixLike => OperatingSystemInfo.UnixLikeLineEnding,
        _ => OperatingSystemInfo.CurrentLineEnding
    };
}