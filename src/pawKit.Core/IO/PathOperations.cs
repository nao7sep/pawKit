namespace pawKit.Core.IO;

using pawKit.Core.Platform;

public static class PathOperations
{
    /// <summary>
    /// The first segment must be an absolute path; otherwise, an exception will be thrown.
    /// </summary>
    public static string Combine(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        ValidateSegments(segments);

        if (!Path.IsPathFullyQualified(segments[0]))
            throw new ArgumentException("The first segment must be an absolute path.", nameof(segments));

        return CombineSegments(segments, type);
    }

    /// <summary>
    /// The first segment may be a relative path, and no exceptions will occur as a result.
    /// </summary>
    public static string Join(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        ValidateSegments(segments);
        return CombineSegments(segments, type);
    }

    private static void ValidateSegments(string[] segments)
    {
        if (segments == null)
            throw new ArgumentNullException(nameof(segments));

        if (segments.Length < 2)
            throw new ArgumentException("At least two segments are required.", nameof(segments));

        if (segments.Any(segment => string.IsNullOrWhiteSpace(segment)))
            throw new ArgumentException("No segment can be null or whitespace.", nameof(segments));
    }

    private static string CombineSegments(string[] segments, DirectorySeparatorType type)
    {
        List<string> processedSegments = [];

        for (int index = 0; index < segments.Length; index++)
        {
            if (index == 0)
                processedSegments.Add(segments[index].Trim().TrimEnd(DirectorySeparatorValues.DirectorySeparators));
            else
                processedSegments.Add(segments[index].Trim().Trim(DirectorySeparatorValues.DirectorySeparators));
        }

        return string.Join(DirectorySeparatorValues.GetDirectorySeparator(type), processedSegments);
    }
}