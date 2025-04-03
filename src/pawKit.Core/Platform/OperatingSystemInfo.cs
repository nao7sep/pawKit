using System.Runtime.InteropServices;
using System.Text;

namespace pawKit.Core.Platform;

public static class OperatingSystemInfo
{
    private static readonly Lazy<OperatingSystemType> _currentOSType = new(DetectOperatingSystem);

    public static OperatingSystemType CurrentOS => _currentOSType.Value;

    public static bool IsWindows => CurrentOS == OperatingSystemType.Windows;

    public static bool IsLinux => CurrentOS == OperatingSystemType.Linux;

    public static bool IsMacOS => CurrentOS == OperatingSystemType.MacOS;

    public static bool IsFreeBSD => CurrentOS == OperatingSystemType.FreeBSD;

    public static bool IsUnixLike => IsLinux || IsMacOS || IsFreeBSD;

    public const char WindowsDirectorySeparator = '\\';

    public const char UnixLikeDirectorySeparator = '/';

    public static char CurrentDirectorySeparator => IsWindows ? WindowsDirectorySeparator : UnixLikeDirectorySeparator;

    public static char CurrentAltDirectorySeparator => IsWindows ? UnixLikeDirectorySeparator : WindowsDirectorySeparator;

    public const char WindowsPathSeparator = ';';

    public const char UnixLikePathSeparator = ':';

    /// <summary>
    /// Gets the path separator character for the current operating system.
    /// This is used to separate paths in environment variables like PATH.
    /// </summary>
    public static char CurrentPathSeparator => IsWindows ? WindowsPathSeparator : UnixLikePathSeparator;

    public const string WindowsLineEnding = "\r\n";

    public const string UnixLikeLineEnding = "\n";

    public static string CurrentLineEnding => IsWindows ? WindowsLineEnding : UnixLikeLineEnding;

    private static OperatingSystemType DetectOperatingSystem()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return OperatingSystemType.Windows;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return OperatingSystemType.Linux;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return OperatingSystemType.MacOS;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            return OperatingSystemType.FreeBSD;

        return OperatingSystemType.Unknown;
    }

    public static string NormalizeDirectorySeparators(string path)
    {
        return NormalizeDirectorySeparators(path, CurrentDirectorySeparator);
    }

    public static string NormalizeDirectorySeparators(string path, char targetSeparator)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        return path.Replace(WindowsDirectorySeparator, targetSeparator)
                   .Replace(UnixLikeDirectorySeparator, targetSeparator);
    }

    public static string NormalizeToWindowsSeparators(string path)
    {
        return NormalizeDirectorySeparators(path, WindowsDirectorySeparator);
    }

    public static string NormalizeToUnixLikeSeparators(string path)
    {
        return NormalizeDirectorySeparators(path, UnixLikeDirectorySeparator);
    }

    public static string NormalizeLineEndings(string text)
    {
        return NormalizeLineEndings(text, CurrentLineEnding);
    }

    public static string NormalizeLineEndings(string text, string targetLineEnding)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        using var reader = new StringReader(text);
        var builder = new StringBuilder();
        string? line;
        bool isFirstLine = true;

        while ((line = reader.ReadLine()) != null)
        {
            if (!isFirstLine)
                builder.Append(targetLineEnding);
            else
                isFirstLine = false;

            builder.Append(line);
        }

        return builder.ToString();
    }

    public static string NormalizeToWindowsLineEndings(string text)
    {
        return NormalizeLineEndings(text, WindowsLineEnding);
    }

    public static string NormalizeToUnixLikeLineEndings(string text)
    {
        return NormalizeLineEndings(text, UnixLikeLineEnding);
    }
}