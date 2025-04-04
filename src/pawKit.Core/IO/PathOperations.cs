namespace pawKit.Core.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using pawKit.Core.Platform;
using pawKit.Core.Text;

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
    /// Combines path segments requiring the first segment to be an absolute path.
    /// </summary>
    public static string Combine(params string[] segments)
    {
        return Combine(segments, DirectorySeparatorType.Default);
    }

    /// <summary>
    /// Joins path segments allowing the first segment to be relative, unlike Combine which requires an absolute path.
    /// </summary>
    public static string Join(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        ValidateSegments(segments);
        return CombineSegments(segments, type);
    }

    /// <summary>
    /// Joins path segments allowing the first segment to be relative, unlike Combine which requires an absolute path.
    /// </summary>
    public static string Join(params string[] segments)
    {
        return Join(segments, DirectorySeparatorType.Default);
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
                processedSegments.Add(segments[index].TrimStart().TrimEndWhiteSpaceAnd(DirectorySeparatorValues.DirectorySeparators));
            else
                processedSegments.Add(segments[index].TrimWhiteSpaceAnd(DirectorySeparatorValues.DirectorySeparators));
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
    public static string Normalize(string path, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        if (Path.IsPathFullyQualified(path))
            return DirectorySeparatorValues.NormalizeDirectorySeparators(Path.GetFullPath(path), type);

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
                    throw new InvalidOperationException("Cannot navigate above the root of a relative path.");
            }

            else
                resultSegments.Add(segment);
        }

        char separator = DirectorySeparatorValues.GetDirectorySeparator(type);
        return DirectorySeparatorValues.NormalizeDirectorySeparators(string.Join(separator, resultSegments), type);
    }

    /// <summary>
    /// Combines path segments requiring the first segment to be an absolute path, then normalizes the result.
    /// </summary>
    /// <remarks>
    /// Use this method when working with absolute paths and you need to ensure the result is in canonical form.
    /// This method will throw an exception if the first segment is not an absolute path.
    /// </remarks>
    public static string CombineAndNormalize(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        string combinedPath = Combine(segments, type);
        return Normalize(combinedPath, type);
    }

    /// <summary>
    /// Combines path segments requiring the first segment to be an absolute path, then normalizes the result.
    /// </summary>
    /// <remarks>
    /// Use this method when working with absolute paths and you need to ensure the result is in canonical form.
    /// This method will throw an exception if the first segment is not an absolute path.
    /// </remarks>
    public static string CombineAndNormalize(params string[] segments)
    {
        return CombineAndNormalize(segments, DirectorySeparatorType.Default);
    }

    /// <summary>
    /// Joins path segments allowing the first segment to be relative, then normalizes the result.
    /// </summary>
    /// <remarks>
    /// Use this method when working with relative paths or when you're not certain if the first segment
    /// is absolute. This method is more flexible than CombineAndNormalize but may not enforce the same
    /// path structure guarantees.
    /// </remarks>
    public static string JoinAndNormalize(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        string joinedPath = Join(segments, type);
        return Normalize(joinedPath, type);
    }

    /// <summary>
    /// Joins path segments allowing the first segment to be relative, then normalizes the result.
    /// </summary>
    /// <remarks>
    /// Use this method when working with relative paths or when you're not certain if the first segment
    /// is absolute. This method is more flexible than CombineAndNormalize but may not enforce the same
    /// path structure guarantees.
    /// </remarks>
    public static string JoinAndNormalize(params string[] segments)
    {
        return JoinAndNormalize(segments, DirectorySeparatorType.Default);
    }
}