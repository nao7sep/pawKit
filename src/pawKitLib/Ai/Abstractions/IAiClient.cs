using pawKitLib.Ai.Sessions;

namespace pawKitLib.Ai.Abstractions;

/// <summary>
/// Defines the contract for a client that interacts with a large language model.
/// </summary>
public interface IAiClient
{
    /// <summary>
    /// Sends the conversation history and inference parameters to the AI model and gets the next message.
    /// </summary>
    /// <param name="session">The current state of the conversation session.</param>
    /// <param name="parameters">The provider-specific parameters for this inference request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A new <see cref="AiMessage"/> from the assistant.</returns>
    Task<AiMessage> GetCompletionAsync(AiSession session, InferenceParameters parameters, CancellationToken cancellationToken = default);
}