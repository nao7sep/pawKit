using pawKitLib.Ai.Sessions;
using pawKitLib.Ai.Content;

namespace pawKitLib.Ai.Abstractions;

/// <summary>
/// Defines a contract for resolving resource references into a provider-ready format.
/// </summary>
/// <remarks>
/// The responsibility of this interface is to handle any resource that requires client-side processing
/// before being sent to the AI provider. The primary example is resolving a <see cref="ResourceKind.LocalPath"/>
/// into <see cref="ResourceKind.InlineBase64"/>. Other resource kinds that are handled by the provider
/// (e.g., <see cref="ResourceKind.RemoteUrl"/>) can be passed through without modification.
/// </remarks>
public interface IResourceResolver
{
    /// <summary>
    /// Resolves a resource reference, for example, by reading a local file into a Base64 string.
    /// </summary>
    /// <param name="resourceRef">The resource reference to resolve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A new <see cref="ResourceRef"/> with the resolved content.</returns>
    Task<ResourceRef> ResolveAsync(ResourceRef resourceRef, CancellationToken cancellationToken = default);
}