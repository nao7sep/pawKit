using pawKitLib.Ai.Sessions;

namespace pawKitLib.Ai.Abstractions;

/// <summary>
/// Defines the contract for a client that supports streaming responses from a large language model.
/// </summary>
public interface IStreamingAiClient
{
    /// <summary>
    /// Sends a request to the AI model and streams the response back as a sequence of partial updates.
    /// </summary>
    /// <param name="context">The prepared request context.</param>
    /// <param name="parameters">The inference parameters for the request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An asynchronous stream of <see cref="StreamingPart"/> updates.</returns>
    IAsyncEnumerable<StreamingPart> StreamCompletionAsync(AiRequestContext context, InferenceParameters parameters, CancellationToken cancellationToken = default);
}