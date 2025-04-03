using System.Runtime.InteropServices;

namespace pawKit.Core.Platform;

/// <summary>
/// Provides information and operations related to the operating system.
/// </summary>
public static class OperatingSystemInfo
{
    private static readonly Lazy <OperatingSystemType> _currentOSType = new (DetectOperatingSystem);

    /// <summary>
    /// Gets the current operating system type.
    /// </summary>
    public static OperatingSystemType CurrentOS => _currentOSType.Value;

    /// <summary>
    /// Determines if the current operating system is Windows.
    /// </summary>
    public static bool IsWindows => CurrentOS == OperatingSystemType.Windows;

    /// <summary>
    /// Determines if the current operating system is Linux.
    /// </summary>
    public static bool IsLinux => CurrentOS == OperatingSystemType.Linux;

    /// <summary>
    /// Determines if the current operating system is macOS.
    /// </summary>
    public static bool IsMacOS => CurrentOS == OperatingSystemType.MacOS;

    /// <summary>
    /// Determines if the current operating system is FreeBSD.
    /// </summary>
    public static bool IsFreeBSD => CurrentOS == OperatingSystemType.FreeBSD;

    /// <summary>
    /// Determines if the current operating system is Unix-like (Linux, macOS, or FreeBSD).
    /// </summary>
    public static bool IsUnixLike => IsLinux || IsMacOS || IsFreeBSD;

    /// <summary>
    /// Gets the default directory separator character for the current operating system.
    /// </summary>
    public static char DirectorySeparator => IsWindows ? '\\' : '/';

    /// <summary>
    /// Gets the alternative directory separator character for the current operating system.
    /// </summary>
    public static char AltDirectorySeparator => IsWindows ? '/' : '\\';

    /// <summary>
    /// Gets the path separator character for the current operating system.
    /// This is used to separate paths in environment variables like PATH.
    /// </summary>
    public static char PathSeparator => IsWindows ? ';' : ':';

    /// <summary>
    /// Gets the line ending string for the current operating system.
    /// </summary>
    public static string LineEnding => IsWindows ? "\r\n" : "\n";

    /// <summary>
    /// Detects the current operating system type.
    /// </summary>
    /// <returns>The detected operating system type.</returns>
    private static OperatingSystemType DetectOperatingSystem ()
    {
        if (RuntimeInformation.IsOSPlatform (OSPlatform.Windows))
            return OperatingSystemType.Windows;

        if (RuntimeInformation.IsOSPlatform (OSPlatform.Linux))
            return OperatingSystemType.Linux;

        if (RuntimeInformation.IsOSPlatform (OSPlatform.OSX))
            return OperatingSystemType.MacOS;

        if (RuntimeInformation.IsOSPlatform (OSPlatform.FreeBSD))
            return OperatingSystemType.FreeBSD;

        return OperatingSystemType.Unknown;
    }

    /// <summary>
    /// Normalizes a path according to the current operating system's conventions.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <returns>The normalized path.</returns>
    public static string NormalizePath (string path)
    {
        if (string.IsNullOrEmpty (path))
            return path;

        // Replace separators with the OS-specific separator
        return path.Replace ('/', DirectorySeparator).Replace ('\\', DirectorySeparator);
    }

    /// <summary>
    /// Converts a path to use forward slashes, regardless of the operating system.
    /// This is useful for paths that need to be used in URLs or cross-platform contexts.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The path with forward slashes.</returns>
    public static string ToForwardSlashPath (string path)
    {
        if (string.IsNullOrEmpty (path))
            return path;

        return path.Replace ('\\', '/');
    }

    /// <summary>
    /// Converts a path to use backslashes, regardless of the operating system.
    /// This is useful for Windows-specific APIs or contexts.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The path with backslashes.</returns>
    public static string ToBackslashPath (string path)
    {
        if (string.IsNullOrEmpty (path))
            return path;

        return path.Replace ('/', '\\');
    }
}
