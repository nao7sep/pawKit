using System.Runtime.InteropServices;

namespace pawKit.Core.Platform;

public static class OperatingSystemInfo
{
    private static readonly Lazy<OperatingSystemType> _currentOS = new(() => DetectOperatingSystem());

    public static OperatingSystemType CurrentOS => _currentOS.Value;

    public static bool IsWindows => CurrentOS == OperatingSystemType.Windows;

    public static bool IsLinux => CurrentOS == OperatingSystemType.Linux;

    public static bool IsMacOS => CurrentOS == OperatingSystemType.MacOS;

    public static bool IsFreeBSD => CurrentOS == OperatingSystemType.FreeBSD;

    public static bool IsUnixLike => IsLinux || IsMacOS || IsFreeBSD;

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
}