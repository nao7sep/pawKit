using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

/// <summary>
/// Orchestrates multi-step tool calling conversations with OpenAI.
/// Handles the complete flow: chat -> tool calls -> tool execution -> result injection -> continuation.
/// </summary>
public class OpenAiToolCallOrchestrator
{
    private readonly ILogger<OpenAiToolCallOrchestrator> _logger;
    private readonly OpenAiChatCompleter _chatCompleter;
    private readonly OpenAiToolCallHandler _toolCallHandler;

    public OpenAiToolCallOrchestrator(
        ILogger<OpenAiToolCallOrchestrator> logger,
        OpenAiChatCompleter chatCompleter,
        OpenAiToolCallHandler toolCallHandler)
    {
        _logger = logger;
        _chatCompleter = chatCompleter;
        _toolCallHandler = toolCallHandler;
    }

    /// <summary>
    /// Executes a complete tool calling conversation, handling multiple rounds of tool calls automatically.
    /// Returns the final assistant response after all tool calls are resolved.
    /// </summary>
    public async Task<OpenAiChatCompletionResponseDto> CompleteWithToolsAsync(
        OpenAiChatCompletionRequestDto request,
        int maxToolCallRounds = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure tools are included in the request
            if (request.Tools == null || request.Tools.Count == 0)
            {
                request.Tools = _toolCallHandler.GetToolDefinitions();
            }

            var conversationMessages = request.Messages.ToList();

            // Update the original request to include tools and current messages
            request.Messages = conversationMessages;
            if (request.Tools == null || request.Tools.Count == 0)
            {
                request.Tools = _toolCallHandler.GetToolDefinitions();
            }

            for (int round = 0; round < maxToolCallRounds; round++)
            {
                // Get response from OpenAI
                var response = await _chatCompleter.CompleteAsync(request, cancellationToken);

                // Check if the response contains tool calls
                if (!_toolCallHandler.HasToolCalls(response))
                {
                    return response;
                }

                // Extract and execute tool calls
                var toolCalls = _toolCallHandler.ExtractToolCalls(response);
                var toolResults = await _toolCallHandler.ExecuteToolCallsAsync(toolCalls);

                // Add assistant message with tool calls to conversation
                var assistantMessage = response.Choices.First().Message!;
                conversationMessages.Add(assistantMessage);

                // Add tool result messages to conversation
                var toolResultMessages = _toolCallHandler.CreateToolResultMessages(toolResults);
                conversationMessages.AddRange(toolResultMessages);

                // Update request messages for next round
                request.Messages = conversationMessages;
            }

            throw new AiServiceException(
                message: $"Maximum tool call rounds ({maxToolCallRounds}) exceeded without completion",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: null);
        }
        catch (AiServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiServiceException(
                message: "Unexpected error during tool calling orchestration.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }

    /// <summary>
    /// Executes a streaming tool calling conversation with automatic tool call handling.
    /// Yields intermediate responses and handles tool calls transparently.
    /// </summary>
    public async IAsyncEnumerable<OpenAiChatCompletionStreamChunkDto> CompleteStreamWithToolsAsync(
        OpenAiChatCompletionRequestDto request,
        int maxToolCallRounds = 5,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Ensure tools are included in the request
        if (request.Tools == null || request.Tools.Count == 0)
        {
            request.Tools = _toolCallHandler.GetToolDefinitions();
        }

        var conversationMessages = request.Messages.ToList();
        request.Messages = conversationMessages;

        for (int round = 0; round < maxToolCallRounds; round++)
        {
            var toolCalls = new List<OpenAiToolCallDto>();
            var assistantMessage = new OpenAiChatMessageDto { Role = "assistant" };
            var hasToolCalls = false;

            // Stream the response and collect tool calls
            await foreach (var chunk in _chatCompleter.CompleteStreamAsync(request, cancellationToken))
            {
                yield return chunk;

                // Collect tool calls from streaming chunks
                if (chunk.Choices != null)
                {
                    foreach (var choice in chunk.Choices)
                    {
                        if (choice.Delta?.ToolCalls != null)
                        {
                            hasToolCalls = true;
                            // Note: In streaming, tool calls may come in fragments
                            // This is a simplified implementation - production code might need more sophisticated merging
                            toolCalls.AddRange(choice.Delta.ToolCalls);
                        }

                        // Collect assistant message content
                        if (choice.Delta?.Content != null)
                        {
                            assistantMessage.Content = (assistantMessage.Content?.ToString() ?? "") + choice.Delta.Content;
                        }
                    }
                }
            }

            // If no tool calls, we're done
            if (!hasToolCalls || toolCalls.Count == 0)
            {
                yield break;
            }

            var toolResults = await _toolCallHandler.ExecuteToolCallsAsync(toolCalls);

            // Add messages to conversation for next round
            assistantMessage.ToolCalls = toolCalls;
            conversationMessages.Add(assistantMessage);

            var toolResultMessages = _toolCallHandler.CreateToolResultMessages(toolResults);
            conversationMessages.AddRange(toolResultMessages);

            // Update request messages for next round
            request.Messages = conversationMessages;
        }

        throw new AiServiceException(
            message: $"Maximum streaming tool call rounds ({maxToolCallRounds}) exceeded without completion",
            statusCode: null,
            rawResponse: null,
            providerDetails: null,
            innerException: null);
    }

    /// <summary>
    /// Executes a single round of tool calls without automatic orchestration.
    /// Useful when you want manual control over the conversation flow.
    /// </summary>
    public async Task<(OpenAiChatCompletionResponseDto Response, List<OpenAiChatMessageDto>? ToolResultMessages)>
        ExecuteSingleToolCallRoundAsync(
            OpenAiChatCompletionRequestDto request,
            CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure tools are included in the request
            if (request.Tools == null || request.Tools.Count == 0)
            {
                request.Tools = _toolCallHandler.GetToolDefinitions();
            }

            var response = await _chatCompleter.CompleteAsync(request, cancellationToken);

            if (!_toolCallHandler.HasToolCalls(response))
            {
                return (response, null);
            }

            var toolCalls = _toolCallHandler.ExtractToolCalls(response);
            var toolResults = await _toolCallHandler.ExecuteToolCallsAsync(toolCalls);
            var toolResultMessages = _toolCallHandler.CreateToolResultMessages(toolResults);

            return (response, toolResultMessages);
        }
        catch (AiServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiServiceException(
                message: "Unexpected error during single tool call round.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }
}
