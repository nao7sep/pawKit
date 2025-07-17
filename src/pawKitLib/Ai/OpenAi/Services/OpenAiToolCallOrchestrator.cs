using Microsoft.Extensions.Logging;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

/// <summary>
/// Orchestrates multi-step tool calling conversations with OpenAI.
/// Handles the complete flow: chat -> tool calls -> tool execution -> result injection -> continuation.
/// </summary>
public class OpenAiToolCallOrchestrator
{
    private const int DefaultMaxToolCallRounds = 5;

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
        int maxToolCallRounds = DefaultMaxToolCallRounds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure tools are included in the request, adding default tools if none are present.
            request.Tools ??= _toolCallHandler.GetToolDefinitions();
            if (request.Tools.Count == 0)
            {
                request.Tools = _toolCallHandler.GetToolDefinitions();
            }

            var conversationMessages = request.Messages.ToList();
            request.Messages = conversationMessages;

            for (int round = 0; round < maxToolCallRounds; round++)
            {
                // Get response from OpenAI
                var response = await _chatCompleter.CompleteAsync(request, cancellationToken);

                // Check if the response contains tool calls
                if (!_toolCallHandler.HasToolCalls(response))
                {
                    return response;
                }

                // Process tool calls
                await ProcessToolCallsAsync(response, conversationMessages);
            }

            throw CreateAiServiceException($"Maximum tool call rounds ({maxToolCallRounds}) exceeded without completion");
        }
        catch (AiServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw CreateAiServiceException("Unexpected error during tool calling orchestration.", ex);
        }
    }

    /// <summary>
    /// Processes tool calls and updates conversation messages.
    /// </summary>
    private async Task ProcessToolCallsAsync(OpenAiChatCompletionResponseDto response, List<OpenAiChatMessageDto> conversationMessages)
    {
        // Extract and execute tool calls
        var toolCalls = _toolCallHandler.ExtractToolCalls(response);
        var toolResults = await _toolCallHandler.ExecuteToolCallsAsync(toolCalls);

        // Add assistant message with tool calls to conversation
        var firstChoice = response.Choices.FirstOrDefault();
        if (firstChoice?.Message == null)
        {
            throw CreateAiServiceException("Response contains no valid message with tool calls");
        }

        firstChoice.Message.ToolCalls = toolCalls;
        conversationMessages.Add(firstChoice.Message);

        // Add tool result messages
        var toolResultMessages = _toolCallHandler.CreateToolResultMessages(toolResults);
        conversationMessages.AddRange(toolResultMessages);
    }

    /// <summary>
    /// Creates a standardized AiServiceException.
    /// </summary>
    private static AiServiceException CreateAiServiceException(string message, Exception? innerException = null)
    {
        return new AiServiceException(message, null, null, null, innerException);
    }
}
