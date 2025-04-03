namespace pawKit.Core.IO;

using pawKit.Core.Platform;

public static class PathOperations
{
    /// <summary>
    /// The first segment must be an absolute path; otherwise, an exception will be thrown.
    /// </summary>
    public static string Combine(string[] segments, PathSeparatorType separatorType = PathSeparatorType.Default)
    {
        ValidateSegments(segments);

        if (!Path.IsPathFullyQualified(segments[0]))
            throw new ArgumentException("The first segment must be an absolute path.", nameof(segments));

        return CombineSegments(segments, separatorType);
    }

    /// <summary>
    /// The first segment may be a relative path, and no exceptions will occur as a result.
    /// </summary>
    public static string Join(string[] segments, PathSeparatorType separatorType = PathSeparatorType.Default)
    {
        ValidateSegments(segments);
        return CombineSegments(segments, separatorType);
    }

    private static void ValidateSegments(string[] segments)
    {
        if (segments == null)
            throw new ArgumentNullException(nameof(segments));

        if (segments.Length < 2)
            throw new ArgumentException("At least two segments are required.", nameof(segments));

        if (segments.Any(segment => string.IsNullOrWhiteSpace(segment)))
            throw new ArgumentNullException(nameof(segments), "Segments cannot contain null or whitespace values.");
    }

    private static string CombineSegments(string[] segments, PathSeparatorType separatorType)
    {
        List<string> processedSegments = [];

        for (int index = 0; index < segments.Length; index++)
        {
            if (index == 0)
                processedSegments.Add(segments[index].Trim().TrimEnd('/', '\\'));
            else
                processedSegments.Add(segments[index].Trim().Trim('/', '\\'));
        }

        return string.Join(PathSeparatorValues.GetSeparator(separatorType), processedSegments);
    }
}