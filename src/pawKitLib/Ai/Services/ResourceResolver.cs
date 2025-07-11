using System.Security;
using pawKitLib.Ai.Abstractions;
using pawKitLib.Ai.Sessions;

namespace pawKitLib.Ai.Services;

/// <summary>
/// A concrete implementation of <see cref="IResourceResolver"/> that handles resolving local file paths.
/// </summary>
public sealed class ResourceResolver : IResourceResolver
{
    private readonly string _allowedBasePath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceResolver"/> class.
    /// </summary>
    /// <param name="allowedBasePath">The absolute base path from which local files are allowed to be resolved. All file access will be restricted to this directory.</param>
    public ResourceResolver(string allowedBasePath)
    {
        // The security check for path traversal is intentionally centralized here.
        // This class is the designated "gatekeeper" for file system access.
        // By enforcing the check internally, we ensure that any consumer of this service is secure by default.
        _allowedBasePath = Path.GetFullPath(allowedBasePath);
    }

    /// <inheritdoc />
    public async Task<ResourceRef> ResolveAsync(ResourceRef resourceRef, CancellationToken cancellationToken = default)
    {
        if (resourceRef.Kind != ResourceKind.LocalPath)
        {
            // This resource kind does not require resolution by this service.
            return resourceRef;
        }

        if (string.IsNullOrWhiteSpace(resourceRef.Value))
        {
            throw new ArgumentException("LocalPath resource reference must have a non-empty file path.", nameof(resourceRef));
        }

        var fullPath = Path.GetFullPath(Path.Combine(_allowedBasePath, resourceRef.Value));

        if (!fullPath.StartsWith(_allowedBasePath, StringComparison.OrdinalIgnoreCase))
        {
            // This check prevents path traversal attacks (e.g., "../../../etc/passwd").
            throw new SecurityException($"Path traversal detected. Access to '{resourceRef.Value}' is forbidden.");
        }

        var fileBytes = await File.ReadAllBytesAsync(fullPath, cancellationToken).ConfigureAwait(false);
        var base64String = Convert.ToBase64String(fileBytes);

        return resourceRef with { Kind = ResourceKind.InlineBase64, Value = base64String };
    }
}