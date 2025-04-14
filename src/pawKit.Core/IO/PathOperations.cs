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
    /// Ensures a path is fully qualified, throwing an exception if it is not.
    /// </summary>
    public static string EnsurePathIsFullyQualified(string path, string? paramName = null)
    {
        if (path == null)
            throw new ArgumentNullException(paramName ?? nameof(path));

        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty or whitespace.", paramName ?? nameof(path));

        if (!Path.IsPathFullyQualified(path))
            throw new ArgumentException("Path must be fully qualified.", paramName ?? nameof(path));

        return path;
    }

    /// <summary>
    /// Combines path segments requiring the first segment to be an absolute path.
    /// </summary>
    public static string CombineAbsolutePath(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        ValidateSegments(segments);

        if (!Path.IsPathFullyQualified(segments[0]))
            throw new ArgumentException("The first segment must be an absolute path.", nameof(segments));

        return CombineSegments(segments, type);
    }

    /// <summary>
    /// Combines path segments requiring the first segment to be an absolute path.
    /// </summary>
    public static string CombineAbsolutePath(params string[] segments)
    {
        return CombineAbsolutePath(segments, DirectorySeparatorType.Default);
    }

    /// <summary>
    /// Joins path segments allowing the first segment to be relative, unlike Combine which requires an absolute path.
    /// </summary>
    public static string JoinPathSegments(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        ValidateSegments(segments);
        return CombineSegments(segments, type);
    }

    /// <summary>
    /// Joins path segments allowing the first segment to be relative, unlike Combine which requires an absolute path.
    /// </summary>
    public static string JoinPathSegments(params string[] segments)
    {
        return JoinPathSegments(segments, DirectorySeparatorType.Default);
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
        List<string> trimmedSegments = [];

        for (int index = 0; index < segments.Length; index++)
        {
            string trimmedSegment = index == 0 ?
                segments[index].TrimStart().TrimEndWhiteSpaceAnd(DirectorySeparatorValues.DirectorySeparators) :
                segments[index].TrimWhiteSpaceAnd(DirectorySeparatorValues.DirectorySeparators);

            // This validation is crucial as segments are directly provided by the developer.
            // They must ensure no item in the string array is invalid (null or empty) after trimming.
            // Unlike NormalizePath which handles repeated separators as a single separator,
            // here we explicitly require each segment to be meaningful.

            // Note: A trimmed segment containing directory separators (e.g., "dir/subdir") is
            // accepted because relative structured paths can be safely combined. The path integrity
            // is maintained as these structures are preserved in the final path composition.

            if (string.IsNullOrEmpty(trimmedSegment))
                throw new ArgumentException("No segment can be null or empty.", nameof(segments));

            trimmedSegments.Add(trimmedSegment);
        }

        return string.Join(DirectorySeparatorValues.GetDirectorySeparator(type), trimmedSegments);
    }

    /// <summary>
    /// Normalizes a path by resolving "." and ".." segments to produce a canonical form.
    /// </summary>
    /// <remarks>
    /// This method provides cross-platform path normalization with these behaviors:
    /// - Leverages system Path.GetFullPath() for absolute paths to handle platform-specific edge cases.
    /// - Manually processes relative paths since .NET doesn't provide a built-in solution.
    /// - Throws an exception when attempting to navigate above the root of a relative path.
    /// - If preserving ".." segments is required, combine with a base path before normalization.
    /// </remarks>
    public static string NormalizePath(string path, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        if (Path.IsPathFullyQualified(path))
            return DirectorySeparatorValues.NormalizeDirectorySeparators(Path.GetFullPath(path), type);

        // Normalize input by removing redundant separators and whitespace before processing.
        // Repeated separators (like "dir//file") are considered as a single separator, not an empty directory name.
        // Unlike CombineSegments which throws an exception for empty segments, here we handle common path format
        // issues quietly since paths may come from external sources or user input.

        string[] segments = path.Split(DirectorySeparatorValues.DirectorySeparators,
            StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        List<string> resultSegments = [];

        for (int index = 0; index < segments.Length; index++)
        {
            string segment = segments[index];

            // Skip current directory references as they don't affect the path.
            if (segment == ".")
                continue;

            if (segment == "..")
            {
                if (resultSegments.Count > 0)
                    // Navigate up by removing the previous path segment.
                    resultSegments.RemoveAt(resultSegments.Count - 1);
                else
                    // Prevent navigation above the root of a relative path - this maintains path integrity.
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
    public static string CombineAndNormalizeAbsolutePath(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        string combinedPath = CombineAbsolutePath(segments, type);
        return NormalizePath(combinedPath, type);
    }

    /// <summary>
    /// Combines path segments requiring the first segment to be an absolute path, then normalizes the result.
    /// </summary>
    /// <remarks>
    /// Use this method when working with absolute paths and you need to ensure the result is in canonical form.
    /// This method will throw an exception if the first segment is not an absolute path.
    /// </remarks>
    public static string CombineAndNormalizeAbsolutePath(params string[] segments)
    {
        return CombineAndNormalizeAbsolutePath(segments, DirectorySeparatorType.Default);
    }

    /// <summary>
    /// Joins path segments allowing the first segment to be relative, then normalizes the result.
    /// </summary>
    /// <remarks>
    /// Use this method when working with relative paths or when you're not certain if the first segment
    /// is absolute. This method is more flexible than CombineAndNormalize but may not enforce the same
    /// path structure guarantees.
    /// </remarks>
    public static string JoinAndNormalizePathSegments(string[] segments, DirectorySeparatorType type = DirectorySeparatorType.Default)
    {
        string joinedPath = JoinPathSegments(segments, type);
        return NormalizePath(joinedPath, type);
    }

    /// <summary>
    /// Joins path segments allowing the first segment to be relative, then normalizes the result.
    /// </summary>
    /// <remarks>
    /// Use this method when working with relative paths or when you're not certain if the first segment
    /// is absolute. This method is more flexible than CombineAndNormalize but may not enforce the same
    /// path structure guarantees.
    /// </remarks>
    public static string JoinAndNormalizePathSegments(params string[] segments)
    {
        return JoinAndNormalizePathSegments(segments, DirectorySeparatorType.Default);
    }
}