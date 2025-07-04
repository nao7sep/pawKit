using Microsoft.Extensions.Logging;

namespace PawKitLib.Services;

/// <summary>
/// Core service class for pawKit library functionality.
/// </summary>
public class PawKitService
{
    private readonly ILogger<PawKitService> _logger;

    /// <summary>
    /// Initializes a new instance of the PawKitService class.
    /// </summary>
    /// <param name="logger">The logger instance for this service.</param>
    public PawKitService(ILogger<PawKitService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the logger instance used by this service.
    /// </summary>
    protected ILogger<PawKitService> Logger => _logger;

    // Add your service methods here
}