using pawKitLib.Ai.Abstractions;
using pawKitLib.Ai.Sessions;

namespace pawKitLib.Ai.Services;

/// <summary>
/// A concrete implementation of <see cref="IResourceResolver"/> that handles resolving local file paths.
/// </summary>
public sealed class ResourceResolver : IResourceResolver
{
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

        var fileBytes = await File.ReadAllBytesAsync(resourceRef.Value, cancellationToken).ConfigureAwait(false);
        var base64String = Convert.ToBase64String(fileBytes);

        return resourceRef with { Kind = ResourceKind.InlineBase64, Value = base64String };
    }
}