using pawKit.Core.Platform;

namespace pawKit.Core.IO;

public static class DirectorySeparatorValues
{
    public static char GetSeparator(DirectorySeparatorType separatorType) => separatorType switch
    {
        DirectorySeparatorType.Windows => OperatingSystemInfo.WindowsDirectorySeparator,
        DirectorySeparatorType.UnixLike => OperatingSystemInfo.UnixLikeDirectorySeparator,
        _ => OperatingSystemInfo.CurrentDirectorySeparator
    };
}