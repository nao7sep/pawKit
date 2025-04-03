namespace pawKit.Core.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pawKit.Core.Platform;

public static class PathOperations
{
    /// <summary>
    /// Combines path segments requiring the first segment to be an absolute path.
    /// </summary>
    public static string Combine(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        ValidateSegments(segments);

        if (!Path.IsPathFullyQualified(segments[0]))
            throw new ArgumentException("The first segment must be an absolute path.", nameof(segments));

        return CombineSegments(segments, type);
    }

    /// <summary>
    /// Joins path segments allowing the first segment to be relative, unlike Combine which requires an absolute path.
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

    /// <summary>
    /// Normalizes a path by resolving "." and ".." segments to produce a canonical form.
    /// </summary>
    /// <remarks>
    /// This method provides cross-platform path normalization with these behaviors:
    /// - Leverages system Path.GetFullPath() for absolute paths to handle platform-specific edge cases
    /// - Manually processes relative paths since .NET doesn't provide a built-in solution
    /// - Throws an exception when attempting to navigate above the root of a relative path
    /// - If preserving ".." segments is required, combine with a base path before normalization
    /// </remarks>
    public static string NormalizePath(string path, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        if (Path.IsPathFullyQualified(path))
            return Path.GetFullPath(path);

        // Normalize input by removing redundant separators and whitespace before processing
        string[] segments = path.Split(DirectorySeparatorValues.DirectorySeparators,
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        List<string> resultSegments = [];

        for (int index = 0; index < segments.Length; index++)
        {
            string segment = segments[index];

            // Skip current directory references as they don't affect the path
            if (segment == ".")
                continue;

            if (segment == "..")
            {
                if (resultSegments.Count > 0)
                    // Navigate up by removing the previous path segment
                    resultSegments.RemoveAt(resultSegments.Count - 1);
                else
                    // Prevent navigation above the root of a relative path - this maintains path integrity
                    throw new InvalidOperationException("Cannot go up from the root directory.");
            }

            else
                resultSegments.Add(segment);
        }

        char separator = DirectorySeparatorValues.GetDirectorySeparator(type);
        return string.Join(separator, resultSegments);
    }
}