using System.Collections.Immutable;
using pawKitLib.Ai.Abstractions;
using pawKitLib.Ai.Sessions;
using pawKitLib.Ai.Requests;
using pawKitLib.Ai.Content;

namespace pawKitLib.Ai.Mocks;

/// <summary>
/// A mock implementation of <see cref="IAiClient"/> for testing and development.
/// This client does not make any network calls and returns a predictable, hardcoded response.
/// </summary>
public sealed class MockAiClient : IAiClient
{
    /// <inheritdoc />
    public Task<IReadOnlyList<AiMessage>> GetCompletionAsync(AiRequestContext context, InferenceParameters parameters, CancellationToken cancellationToken = default)
    {
        var responseText = $"Mock response to a request with {context.Messages.Count} user/assistant messages. The system prompt was: '{context.SystemPrompt ?? "none"}'. Model requested: '{parameters.ModelId ?? "default"}'.";

        var messages = Enumerable.Range(0, parameters.N ?? 1)
            .Select(_ => new AiMessage
            {
                Id = Guid.NewGuid(),
                Role = MessageRole.Assistant,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                Parts = ImmutableList.Create<IContentPart>(new TextContentPart(responseText))
            })
            .ToList();

        return Task.FromResult((IReadOnlyList<AiMessage>)messages);
    }
}