﻿using pawKitLib.Ai.Sessions;
using pawKitLib.Ai.Requests;
using System.Threading;

namespace pawKitLib.Ai.Abstractions;

/// <summary>
/// Defines the contract for a client that interacts with a large language model.
/// </summary>
/// <remarks>
/// This interface is responsible for **API protocol translation**. It takes a fully prepared
/// <see cref="AiRequestContext"/> and the <see cref="InferenceParameters"/> and translates them
/// into a concrete HTTP request for a specific provider. It should not contain any logic
/// for building or modifying the request context itself; that is the responsibility of the <see cref="IRequestContextBuilder"/>.
/// </remarks>
public interface IAiClient
{
    /// <summary>
    /// Sends the conversation history and inference parameters to the AI model and gets the next message.
    /// </summary>
    /// <param name="context">The prepared request context, containing the final messages and tools.</param>
    /// <param name="parameters">The provider-specific parameters for this inference request.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A new <see cref="AiMessage"/> from the assistant.</returns>
    Task<AiMessage> GetCompletionAsync(AiRequestContext context, InferenceParameters parameters, CancellationToken cancellationToken = default);
}