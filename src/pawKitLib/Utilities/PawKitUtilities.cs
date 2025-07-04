using Microsoft.Extensions.Logging;
using PawKitLib.Logging;

namespace PawKitLib.Utilities;

/// <summary>
/// Static utility class providing common functionality for pawKit library.
/// </summary>
public static class PawKitUtilities
{
    private static readonly Lazy<ILogger> _logger = new Lazy<ILogger>(() => PawKitLog.CreateLogger(nameof(PawKitUtilities)));

    /// <summary>
    /// Gets the logger instance used by this utility class.
    /// </summary>
    public static ILogger Logger => _logger.Value;

    // Add your utility methods here
}