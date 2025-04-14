namespace pawKit.Core.Platform;

using System;

public static class DirectorySeparatorValues
{
    public const char WindowsDirectorySeparator = '\\';

    public const char UnixLikeDirectorySeparator = '/';

    public static readonly char[] DirectorySeparators = [WindowsDirectorySeparator, UnixLikeDirectorySeparator];

    private static readonly Lazy<char> _defaultDirectorySeparator = new(() =>
        OperatingSystemInfo.IsWindows ? WindowsDirectorySeparator : UnixLikeDirectorySeparator);

    private static readonly Lazy<char> _defaultAltDirectorySeparator = new(() =>
        OperatingSystemInfo.IsWindows ? UnixLikeDirectorySeparator : WindowsDirectorySeparator);

    public static char DefaultDirectorySeparator => _defaultDirectorySeparator.Value;

    public static char DefaultAltDirectorySeparator => _defaultAltDirectorySeparator.Value;

    public static char GetDirectorySeparator(DirectorySeparatorType type) => type switch
    {
        DirectorySeparatorType.Windows => WindowsDirectorySeparator,
        DirectorySeparatorType.UnixLike => UnixLikeDirectorySeparator,
        _ => DefaultDirectorySeparator
    };

    public static string NormalizeDirectorySeparators(string path)
    {
        return NormalizeDirectorySeparators(path, DirectorySeparatorType.Default);
    }

    public static string NormalizeDirectorySeparators(string path, DirectorySeparatorType targetType)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        if (targetType == DirectorySeparatorType.Windows)
            return path.Replace(UnixLikeDirectorySeparator, WindowsDirectorySeparator);
        else if (targetType == DirectorySeparatorType.UnixLike)
            return path.Replace(WindowsDirectorySeparator, UnixLikeDirectorySeparator);
        else
            return path.Replace(DefaultAltDirectorySeparator, DefaultDirectorySeparator);
    }

    public static string NormalizeToWindowsDirectorySeparators(string path)
    {
        return NormalizeDirectorySeparators(path, DirectorySeparatorType.Windows);
    }

    public static string NormalizeToUnixLikeDirectorySeparators(string path)
    {
        return NormalizeDirectorySeparators(path, DirectorySeparatorType.UnixLike);
    }
}