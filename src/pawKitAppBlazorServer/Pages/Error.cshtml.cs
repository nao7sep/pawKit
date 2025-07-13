// This code-behind file provides logic and data for the Error.cshtml Razor Page.
using System.Diagnostics; // For getting request/activity IDs
using Microsoft.AspNetCore.Mvc; // For MVC attributes
using Microsoft.AspNetCore.Mvc.RazorPages; // For Razor PageModel base class

namespace pawKitAppBlazorServer.Pages;

// This error page is shown when an unhandled exception occurs during the processing of an HTTP request (such as when a user loads a page and something fails in the request pipeline).
// It does NOT automatically show errors from background tasks, hosted services, or code running outside the request pipeline.
// Those errors should be handled and logged separately, as this page only responds to web request failures.

// Prevents caching of the error page so users always see the latest error info
// Duration: in seconds; 0 means do not cache at all
// Location: where the response can be cached; None means do not cache anywhere (not client, proxy, or server)
// NoStore: true means do not store the response in any cache (strictest setting)
// This combination is ideal for error pages and sensitive data, ensuring users always see the latest info and never see old or private data
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
// Disables antiforgery protection for this page.
// Antiforgery protection helps prevent Cross-Site Request Forgery (CSRF) attacks by requiring a special token on form submissions and certain requests.
// It's important for pages that accept user input (like login or data entry forms).
// The error page does not process form submissions or sensitive user actions, so antiforgery protection is not needed here.
// Disabling it avoids unnecessary checks and potential errors when displaying the error page.
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    // The unique ID for the current request, useful for debugging and tracing errors
    public string? RequestId { get; set; }

    // True if there is a request ID to show in the UI
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    // Logger for recording error details (can be used for diagnostics)
    // When this ErrorModel is constructed, ASP.NET Core's dependency injection system automatically provides an ILogger<ErrorModel> instance.
    // The DI system does not just pick the first ILogger; it creates a logger specifically for ErrorModel, so logs are categorized by component.
    // If multiple logging providers are registered (e.g., console, file), all are used together, but only one logger instance per type is injected.
    // ILogger is thread-safe, so it can be safely used from multiple threads at the same time.
    private readonly ILogger<ErrorModel> _logger;

    // Constructor receives a logger via dependency injection
    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    // Called when the error page is loaded (GET request)
    public void OnGet()
    {
        // Set the RequestId to the current activity ID or HTTP context trace identifier
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        // Log an error message using structured logging.
        // Structured logs use message templates with named fields (like {RequestId}), making it easy to search, filter, and analyze log data.
        // Always use named placeholders for all variable data to ensure logs are structured.
        // You can add more fields (like user info or exception details) as named parameters for richer structured logs.
        // LogError is used for serious issues that need attention from developers or support staff.
        _logger.LogError(
            "An error occurred while processing the request. RequestId: {RequestId}. This event was logged for diagnostics and support.",
            RequestId
        );
    }
}
