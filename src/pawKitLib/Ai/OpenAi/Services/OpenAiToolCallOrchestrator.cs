using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

// OpenAiToolCallOrchestrator: High-level automation for OpenAI tool-calling conversations
// -----------------------------------------------------------------------------
// This class coordinates the full multi-step process of interacting with OpenAI's function calling API:
//   - Sends chat requests to OpenAI, including available tool schemas.
//   - Detects when OpenAI wants to call a tool, extracts the tool calls, and executes them via the handler.
//   - Injects tool results back into the conversation and continues the loop until OpenAI is done.
//   - Handles multiple rounds of tool calls, error handling, and conversation state management.
//
// This orchestrator is the main entry point for running a complete tool-calling workflow, ensuring that all tool calls
// are resolved and the final assistant response is returned. It depends on OpenAiToolCallHandler for tool execution
// and OpenAiChatCompleter for communication with OpenAI.

/// <summary>
/// Orchestrates multi-step tool calling conversations with OpenAI.
/// Handles the complete flow: chat -> tool calls -> tool execution -> result injection -> continuation.
/// </summary>
// This class is stateless between runs and safe for concurrent use as long as its dependencies are thread-safe.
// It is designed to be the main entry point for running a complete tool-calling workflow with OpenAI.
public class OpenAiToolCallOrchestrator
{
    // The maximum number of tool-calling rounds allowed in a single conversation flow.
    // Each round consists of sending a chat request to OpenAI, checking for tool calls,
    // executing any requested tools, injecting the results, and repeating if needed.
    // This limit prevents infinite loops or runaway conversations if OpenAI keeps requesting tool calls.
    // Five rounds is typically sufficient for most workflows, but this value can be overridden per request.
    private const int DefaultMaxToolCallRounds = 5;

    private readonly ILogger<OpenAiToolCallOrchestrator> _logger;
    private readonly OpenAiChatCompleter _chatCompleter;
    private readonly OpenAiToolCallHandler _toolCallHandler;

    // Dependencies are injected via the constructor and must be provided by the caller.
    // This allows for flexible composition and testing.
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
    // Workflow:
    //   1. Ensure the request includes all available tool schemas.
    //   2. For up to maxToolCallRounds:
    //        a. Send the chat request to OpenAI.
    //        b. If the response contains tool calls, extract and execute them.
    //        c. Inject the tool results as messages and continue the loop.
    //        d. If no tool calls remain, return the final assistant response.
    //   3. If the maximum number of rounds is exceeded, throw an exception to prevent infinite loops.
    // Error handling:
    //   - AiServiceException is rethrown as-is for upstream handling.
    //   - All other exceptions are wrapped in AiServiceException for consistent error reporting.
    public async Task<OpenAiChatCompletionResponseDto> CompleteWithToolsAsync(
        OpenAiChatCompletionRequestDto request,
        int maxToolCallRounds = DefaultMaxToolCallRounds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ensure tools are included in the request, adding default tools if none are present.
            if (request.Tools == null || request.Tools.Count == 0)
            {
                request.Tools = _toolCallHandler.GetToolDefinitions();
            }

            var conversationMessages = request.Messages?.ToList() ?? new List<OpenAiChatMessageDto>();
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

                // Extract and execute tool calls
                var toolCalls = _toolCallHandler.ExtractToolCalls(response);
                var toolResults = await _toolCallHandler.ExecuteToolCallsAsync(toolCalls);

                // Add assistant message with tool calls to conversation
                var firstChoice = response.Choices?.FirstOrDefault();
                if (firstChoice?.Message == null)
                {
                    throw new AiServiceException(
                        message: "Response contains no valid message with tool calls",
                        statusCode: null,
                        rawResponse: null,
                        providerDetails: null,
                        innerException: null);
                }

                firstChoice.Message.ToolCalls = toolCalls;
                conversationMessages.Add(firstChoice.Message);

                // Add tool result messages
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
}
