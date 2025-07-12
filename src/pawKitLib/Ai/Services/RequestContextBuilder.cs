using pawKitLib.Ai.Content;
using System.Collections.Immutable;
using pawKitLib.Ai.Abstractions;
using pawKitLib.Ai.Sessions;
using pawKitLib.Ai.Requests;

namespace pawKitLib.Ai.Services;

/// <summary>
/// A concrete implementation of <see cref="IRequestContextBuilder"/>.
/// </summary>
public sealed class RequestContextBuilder : IRequestContextBuilder
{
    private readonly IResourceResolver _resourceResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestContextBuilder"/> class.
    /// </summary>
    /// <param name="resourceResolver">The resolver for handling resource references.</param>
    public RequestContextBuilder(IResourceResolver resourceResolver)
    {
        _resourceResolver = resourceResolver;
    }

    /// <summary>
    /// Constructs a clean request context by applying overrides and resolving resources.
    /// </summary>
    /// <param name="session">The canonical session state.</param>
    /// <param name="parameters">The inference parameters for the request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The final, provider-ready <see cref="AiRequestContext"/>.</returns>
    public async Task<AiRequestContext> BuildContextAsync(AiSession session, InferenceParameters parameters, CancellationToken cancellationToken = default)
    {
        var finalSystemPrompt = DetermineFinalSystemPrompt(session, parameters);

        var finalMessages = await BuildFinalMessageListAsync(session, cancellationToken).ConfigureAwait(false);

        return new AiRequestContext(
            finalMessages,
            finalSystemPrompt,
            session.AvailableTools
        );
    }

    /// <summary>
    /// Determines the definitive system prompt for the request.
    /// The override from <see cref="InferenceParameters"/> takes precedence over the canonical system message in the session.
    /// </summary>
    /// <param name="session">The current session.</param>
    /// <param name="parameters">The inference parameters for the request.</param>
    /// <returns>The final system prompt string, or null if none is found or defined.</returns>
    private static string? DetermineFinalSystemPrompt(AiSession session, InferenceParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.SystemPromptOverride))
        {
            return parameters.SystemPromptOverride;
        }

        var systemMessage = session.Messages.FirstOrDefault(m => m.Role == MessageRole.System);
        if (systemMessage is null)
        {
            return null;
        }

        var textParts = systemMessage.Parts.OfType<TextContentPart>().Select(p => p.Text);
        var combinedText = string.Join(Environment.NewLine, textParts);

        return string.IsNullOrWhiteSpace(combinedText) ? null : combinedText;
    }

    /// <summary>
    /// Builds the final list of messages to be sent to the provider.
    /// This method filters out the canonical system message and applies all context overrides.
    /// </summary>
    /// <param name="session">The current session.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An immutable list of messages ready for the provider.</returns>
    private async Task<ImmutableList<AiMessage>> BuildFinalMessageListAsync(AiSession session, CancellationToken cancellationToken)
    {
        var finalMessages = ImmutableList.CreateBuilder<AiMessage>();
        var processableMessages = session.Messages.Where(m => m.Role != MessageRole.System);

        foreach (var message in processableMessages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (session.ContextOverrides.TryGetValue(message.Id, out var overrideInfo))
            {
                if (overrideInfo.IsExcluded)
                {
                    continue; // Message is explicitly excluded.
                }

                if (overrideInfo.AlternateParts is not null)
                {
                    var resolvedParts = await ResolveMessagePartsAsync(overrideInfo.AlternateParts, cancellationToken).ConfigureAwait(false);
                    finalMessages.Add(message with { Parts = resolvedParts });
                    continue; // Message parts were replaced.
                }
            }

            // No override, or override didn't apply. Process original parts.
            if (message.Parts.Any(p => p is MediaContentPart))
            {
                var resolvedOriginalParts = await ResolveMessagePartsAsync(message.Parts, cancellationToken).ConfigureAwait(false);
                finalMessages.Add(message with { Parts = resolvedOriginalParts });
            }
            else
            {
                finalMessages.Add(message); // No parts needed resolution.
            }
        }

        return finalMessages.ToImmutable();
    }

    /// <summary>
    /// Resolves all resource-based content parts within a given list of parts.
    /// </summary>
    /// <param name="parts">The list of content parts to process.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A new immutable list with all media parts resolved.</returns>
    private async Task<ImmutableList<IContentPart>> ResolveMessagePartsAsync(ImmutableList<IContentPart> parts, CancellationToken cancellationToken)
    {
        var resolvedPartsBuilder = ImmutableList.CreateBuilder<IContentPart>();
        foreach (var part in parts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (part is MediaContentPart mediaPart)
            {
                var resolvedResource = await _resourceResolver.ResolveAsync(mediaPart.Resource, cancellationToken).ConfigureAwait(false);
                resolvedPartsBuilder.Add(mediaPart with { Resource = resolvedResource });
            }
            else
            {
                resolvedPartsBuilder.Add(part);
            }
        }
        return resolvedPartsBuilder.ToImmutable();
    }
}