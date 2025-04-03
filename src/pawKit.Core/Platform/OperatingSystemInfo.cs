using System.Runtime.InteropServices;

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

    public static char DirectorySeparator => IsWindows ? '\\' : '/';

    public static char AltDirectorySeparator => IsWindows ? '/' : '\\';

    /// <summary>
    /// Gets the path separator character for the current operating system.
    /// This is used to separate paths in environment variables like PATH.
    /// </summary>
    public static char PathSeparator => IsWindows ? ';' : ':';

    public static string LineEnding => IsWindows ? "\r\n" : "\n";

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

    public static string NormalizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return path.Replace('/', DirectorySeparator).Replace('\\', DirectorySeparator);
    }

    public static string ToForwardSlashPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return path.Replace('\\', '/');
    }

    public static string ToBackslashPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        return path.Replace('/', '\\');
    }
}