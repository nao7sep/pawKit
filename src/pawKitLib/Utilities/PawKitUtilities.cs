using Microsoft.Extensions.Logging;
using PawKitLib.Logging;

namespace PawKitLib.Utilities;

/// <summary>
/// Static utility class providing common functionality for pawKit library.
/// </summary>
public static class PawKitUtilities
{
    private static readonly ILogger _logger = PawKitLog.CreateLogger(nameof(PawKitUtilities));

    /// <summary>
    /// Gets the logger instance used by this utility class.
    /// </summary>
    public static ILogger Logger => _logger;

    // Add your utility methods here
}