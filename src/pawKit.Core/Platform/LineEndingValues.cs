using System.Text;

namespace pawKit.Core.Platform;

public static class LineEndingValues
{
    public const string WindowsLineEnding = "\r\n";

    public const string UnixLikeLineEnding = "\n";

    private static readonly Lazy<string> _defaultLineEnding = new Lazy<string>(() =>
        OperatingSystemInfo.IsWindows ? WindowsLineEnding : UnixLikeLineEnding);

    public static string DefaultLineEnding => _defaultLineEnding.Value;

    public static string GetLineEnding(LineEndingType type) => type switch
    {
        LineEndingType.Windows => WindowsLineEnding,
        LineEndingType.UnixLike => UnixLikeLineEnding,
        _ => DefaultLineEnding
    };

    public static string NormalizeLineEndings(string text)
    {
        return NormalizeLineEndings(text, LineEndingType.Default);
    }

    public static string NormalizeLineEndings(string text, LineEndingType targetType)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        using var reader = new StringReader(text);
        var builder = new StringBuilder();
        string? line;
        bool isFirstLine = true;
        string targetLineEnding = GetLineEnding(targetType);

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
        return NormalizeLineEndings(text, LineEndingType.Windows);
    }

    public static string NormalizeToUnixLikeLineEndings(string text)
    {
        return NormalizeLineEndings(text, LineEndingType.UnixLike);
    }
}