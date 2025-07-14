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
                { ".txt", "text/plain" },
                { ".pdf", "application/pdf" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
                { ".ppt", "application/vnd.ms-powerpoint" },
                { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { ".rtf", "application/rtf" },
                { ".csv", "text/csv" },

                // Image Types
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".bmp", "image/bmp" },
                { ".svg", "image/svg+xml" },
                { ".webp", "image/webp" },
                { ".ico", "image/vnd.microsoft.icon" },

                // Audio Types
                { ".mp3", "audio/mpeg" },
                { ".wav", "audio/wav" },
                { ".ogg", "audio/ogg" },
                { ".m4a", "audio/mp4" },
                { ".aac", "audio/aac" },

                // Video Types
                { ".mp4", "video/mp4" },
                { ".webm", "video/webm" },
                { ".mov", "video/quicktime" },
                { ".avi", "video/x-msvideo" },
                { ".mkv", "video/x-matroska" },

                // Archive Types
                { ".zip", "application/zip" },
                { ".rar", "application/vnd.rar" },
                { ".7z", "application/x-7z-compressed" },
                { ".tar", "application/x-tar" },
                { ".gz", "application/gzip" },

                // Web & Data Types
                { ".html", "text/html" },
                { ".htm", "text/html" },
                { ".css", "text/css" },
                { ".js", "text/javascript" },
                { ".json", "application/json" },
                { ".xml", "application/xml" },
                { ".md", "text/markdown" },

                // Font Types
                { ".ttf", "font/ttf" },
                { ".otf", "font/otf" },
                { ".woff", "font/woff" },
                { ".woff2", "font/woff2" },
            }.ToImmutableDictionary();

        /// <summary>
        /// Gets the MIME type for the given file extension or file name.
        /// Returns null if the extension is unknown.
        /// </summary>
        public static string? GetMimeType(string fileNameOrExtension)
        {
            if (string.IsNullOrWhiteSpace(fileNameOrExtension))
                return null;

            var ext = Path.GetExtension(fileNameOrExtension);

            if (!string.IsNullOrEmpty(ext) && MimeTypes.TryGetValue(ext, out var mime))
                return mime;

            return null;
        }
    }
}
