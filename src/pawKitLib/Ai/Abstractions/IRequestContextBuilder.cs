using pawKitLib.Ai.Sessions;

namespace pawKitLib.Ai.Abstractions;

/// <summary>
/// Defines a contract for building a final, provider-ready request context from a session.
/// </summary>
/// <remarks>
/// This service's responsibility is to prepare the **content payload** for the AI. It applies all
/// session-level rules, such as <see cref="AiSession.ContextOverrides"/>, and resolves any
/// resource references via <see cref="IResourceResolver"/>. It also handles tactical, per-request
/// content modifications, such as applying the <see cref="InferenceParameters.SystemPromptOverride"/>.
/// </remarks>
public interface IRequestContextBuilder
{
    /// <summary>
    /// Constructs a clean request context by applying overrides and resolving resources.
    /// </summary>
    /// <param name="session">The canonical session state.</param>
    /// <param name="parameters">The inference parameters for the request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The final, provider-ready <see cref="AiRequestContext"/>.</returns>
    Task<AiRequestContext> BuildContextAsync(AiSession session, InferenceParameters parameters, CancellationToken cancellationToken = default);
}