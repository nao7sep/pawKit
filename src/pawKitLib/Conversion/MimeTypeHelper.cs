using System;
using System.Collections.Immutable;
using System.IO;

namespace pawKitLib.Conversion
{
    /// <summary>
    /// Provides utility methods for determining MIME types based on file extensions.
    /// Uses an ImmutableDictionary as a static, constant-like lookup table.
    /// </summary>
    public static class MimeTypeHelper
    {
        // Note: We do not use lazy loading for MimeTypes.
        // The dictionary is small, static, and inexpensive to initialize.
        private static readonly ImmutableDictionary<string, string> MimeTypes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                // Document Types
                { ".bak", "application/octet-stream" },
                { ".csv", "text/csv" },
                { ".db", "application/octet-stream" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".epub", "application/epub+zip" },
                { ".ini", "text/plain" },
                { ".log", "text/plain" },
                { ".odt", "application/vnd.oasis.opendocument.text" },
                { ".pdf", "application/pdf" },
                { ".ppt", "application/vnd.ms-powerpoint" },
                { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { ".rtf", "application/rtf" },
                { ".sqlite", "application/x-sqlite3" },
                { ".tsv", "text/tab-separated-values" },
                { ".txt", "text/plain" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },

                // Image Types
                { ".avif", "image/avif" },
                { ".bmp", "image/bmp" },
                { ".emf", "image/emf" },
                { ".gif", "image/gif" },
                { ".heic", "image/heic" },
                { ".ico", "image/vnd.microsoft.icon" },
                { ".jpeg", "image/jpeg" },
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".raw", "image/raw" },
                { ".svg", "image/svg+xml" },
                { ".tif", "image/tiff" },
                { ".tiff", "image/tiff" },
                { ".webp", "image/webp" },
                { ".wmf", "image/wmf" },

                // Audio Types
                { ".aac", "audio/aac" },
                { ".aif", "audio/aiff" },
                { ".aiff", "audio/aiff" },
                { ".flac", "audio/flac" },
                { ".m4a", "audio/mp4" },
                { ".mid", "audio/midi" },
                { ".midi", "audio/midi" },
                { ".mp3", "audio/mpeg" },
                { ".ogg", "audio/ogg" },
                { ".opus", "audio/opus" },
                { ".wav", "audio/wav" },

                // Video Types
                { ".3gp", "video/3gpp" },
                { ".avi", "video/x-msvideo" },
                { ".flv", "video/x-flv" },
                { ".m4v", "video/x-m4v" },
                { ".mkv", "video/x-matroska" },
                { ".mov", "video/quicktime" },
                { ".mp4", "video/mp4" },
                { ".mpeg", "video/mpeg" },
                { ".webm", "video/webm" },
                { ".wmv", "video/x-ms-wmv" },

                // Archive Types
                { ".7z", "application/x-7z-compressed" },
                { ".bz2", "application/x-bzip2" },
                { ".cab", "application/vnd.ms-cab-compressed" },
                { ".gz", "application/gzip" },
                { ".iso", "application/x-iso9660-image" },
                { ".rar", "application/vnd.rar" },
                { ".tar", "application/x-tar" },
                { ".xz", "application/x-xz" },
                { ".zip", "application/zip" },

                // Web & Data Types
                { ".css", "text/css" },
                { ".htm", "text/html" },
                { ".html", "text/html" },
                { ".js", "text/javascript" },
                { ".jsx", "text/jsx" },
                { ".json", "application/json" },
                { ".map", "application/json" },
                { ".md", "text/markdown" },
                { ".ts", "application/typescript" },
                { ".tsx", "text/tsx" },
                { ".xhtml", "application/xhtml+xml" },
                { ".xml", "application/xml" },
                { ".yaml", "application/x-yaml" },
                { ".yml", "application/x-yaml" },

                // Font Types
                { ".eot", "application/vnd.ms-fontobject" },
                { ".otf", "font/otf" },
                { ".sfnt", "font/sfnt" },
                { ".ttf", "font/ttf" },
                { ".woff", "font/woff" },
                { ".woff2", "font/woff2" },

                // Other Types
                { ".bat", "application/x-msdos-program" },
                { ".cmd", "application/x-msdos-program" },
                { ".dll", "application/x-msdownload" },
                { ".exe", "application/vnd.microsoft.portable-executable" },
                { ".msi", "application/x-msi" },
                { ".ps1", "text/x-powershell" },
                { ".sh", "application/x-sh" },
                { ".sys", "application/octet-stream" },
            }.ToImmutableDictionary();

        /// <summary>
        /// The default fallback MIME type for unknown or binary files.
        /// </summary>
        public static string DefaultMimeType => "application/octet-stream";

        /// <summary>
        /// Gets the MIME type for the given file extension or file name.
        /// Returns DefaultMimeType if the extension is unknown and fallbackToDefault is true (default).
        /// Optionally accepts additional extension/MIME mappings as a read-only, case-insensitive dictionary.
        /// The parameter name is intentionally verbose to ensure developers provide a case-insensitive dictionary,
        /// enabling reliable conversion without requiring extra normalization logic within the method.
        /// </summary>
        public static string? GetMimeType(
            string fileNameOrExtension,
            IReadOnlyDictionary<string, string>? additionalMimeTypesCaseInsensitive = null,
            bool fallbackToDefault = true)
        {
            if (string.IsNullOrWhiteSpace(fileNameOrExtension))
                return fallbackToDefault ? DefaultMimeType : null;

            var ext = Path.GetExtension(fileNameOrExtension);
            if (string.IsNullOrEmpty(ext))
                return fallbackToDefault ? DefaultMimeType : null;

            // Check additional mappings first
            if (additionalMimeTypesCaseInsensitive != null && additionalMimeTypesCaseInsensitive.TryGetValue(ext, out var customMime))
                return customMime;

            // Fallback to built-in dictionary
            if (MimeTypes.TryGetValue(ext, out var mime))
                return mime;

            return fallbackToDefault ? DefaultMimeType : null;
        }
    }
}
