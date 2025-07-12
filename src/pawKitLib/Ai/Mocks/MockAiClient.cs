using System.Collections.Generic;
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
        var responseCount = parameters.N ?? 1;
        var baseResponseText = $"Mock response to a request with {context.Messages.Count} user/assistant messages. The system prompt was: '{context.SystemPrompt ?? "none"}'. Model requested: '{parameters.ModelId ?? "default"}'.";

        var messages = new List<AiMessage>();
        for (var i = 0; i < responseCount; i++)
        {
            var responseText = responseCount > 1
                ? $"{baseResponseText} (Choice {i + 1}/{responseCount})"
                : baseResponseText;

            messages.Add(new AiMessage
            {
                Id = Guid.NewGuid(),
                Role = MessageRole.Assistant,
                CreatedAtUtc = DateTimeOffset.UtcNow,
                Parts = ImmutableList.Create<IContentPart>(new TextContentPart(responseText))
            });
        }

        return Task.FromResult<IReadOnlyList<AiMessage>>(messages);
    }
}