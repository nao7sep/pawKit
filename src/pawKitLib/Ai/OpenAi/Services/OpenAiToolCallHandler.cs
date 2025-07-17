using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

// OpenAiToolCallHandler: Core bridge between C# and OpenAI function calling
// ---------------------------------------------------------------
// This class manages the registration and execution of C# methods as "tools"
// that can be called by OpenAI's function calling API. It allows you to:
//   - Register synchronous or asynchronous C# methods as callable tools, each with a name and schema.
//   - Provide OpenAI with a list of available tools and their parameter schemas.
//   - Receive tool call requests from OpenAI, deserialize arguments, invoke the correct C# method, and serialize the result.
//   - Package results as messages for OpenAI to use in ongoing chat conversations.
//   - Manage the set of registered tools (list, check, unregister, clear).
//
// This class is typically used by higher-level orchestrators to automate multi-step tool-calling conversations with OpenAI.

/// <summary>
/// Handles registration and execution of C# functions as OpenAI tools.
/// Provides runtime dispatch for tool calls and result injection back into chat flow.
/// </summary>
public class OpenAiToolCallHandler
{
    private readonly ILogger<OpenAiToolCallHandler> _logger;
    private readonly Dictionary<string, RegisteredTool> _registeredTools = new();

    public OpenAiToolCallHandler(ILogger<OpenAiToolCallHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a C# method as a callable tool for OpenAI.
    /// </summary>
    public void RegisterTool<T>(string name, Func<T, object> handler, OpenAiFunctionDto functionDefinition)
    {
        // Registers a synchronous C# function as a tool callable by OpenAI.
        // The handler is a standard C# function that takes a deserialized argument of type T and returns an object.
        // Arguments from OpenAI are always received as JSON strings and are deserialized to type T before invocation.
        // The result is serialized back to JSON for OpenAI.
        // Although the handler is synchronous, it is wrapped in a Task to fit the async handler signature.
        // Any exceptions are caught and wrapped in an AiServiceException for consistent error handling.

        var registeredTool = new RegisteredTool
        {
            Name = name,
            FunctionDefinition = functionDefinition,
            Handler = (argsJson) =>
            {
                try
                {
                    // Deserialize arguments from JSON string to the expected C# type.
                    var args = JsonSerializer.Deserialize<T>(argsJson);
                    if (args == null)
                    {
                        throw new ArgumentException($"Failed to deserialize arguments for tool '{name}'");
                    }

                    // Execute the provided handler function with the deserialized arguments.
                    var result = handler(args);
                    // Serialize the result back to JSON for OpenAI.
                    return Task.FromResult(JsonSerializer.Serialize(result));
                }
                catch (Exception ex)
                {
                    // Wrap any error in a custom exception for consistent error handling.
                    throw new AiServiceException(
                        message: $"Tool execution failed for '{name}'",
                        statusCode: null,
                        rawResponse: null,
                        providerDetails: null,
                        innerException: ex);
                }
            }
        };

        _registeredTools[name] = registeredTool;
    }

    /// <summary>
    /// Registers an async C# method as a callable tool for OpenAI.
    /// </summary>
    public void RegisterAsyncTool<T>(string name, Func<T, Task<object>> handler, OpenAiFunctionDto functionDefinition)
    {
        // Registers an asynchronous C# function as a tool callable by OpenAI.
        // The handler is an async function that takes a deserialized argument of type T and returns a Task<object>.
        // Arguments from OpenAI are always received as JSON strings and are deserialized to type T before invocation.
        // The result is awaited and then serialized back to JSON for OpenAI.
        // This method should be used for tool logic that is naturally asynchronous (e.g., I/O, network calls).
        // Any exceptions are caught and wrapped in an AiServiceException for consistent error handling.

        var registeredTool = new RegisteredTool
        {
            Name = name,
            FunctionDefinition = functionDefinition,
            Handler = async (argsJson) =>
            {
                try
                {
                    var args = JsonSerializer.Deserialize<T>(argsJson);
                    if (args == null)
                    {
                        throw new ArgumentException($"Failed to deserialize arguments for tool '{name}'");
                    }

                    var result = await handler(args);
                    return JsonSerializer.Serialize(result);
                }
                catch (Exception ex)
                {
                    throw new AiServiceException(
                        message: $"Async tool execution failed for '{name}'",
                        statusCode: null,
                        rawResponse: null,
                        providerDetails: null,
                        innerException: ex);
                }
            }
        };

        _registeredTools[name] = registeredTool;
    }

    /// <summary>
    /// Gets all registered tools as OpenAI tool definitions for use in chat completion requests.
    /// </summary>
    public List<OpenAiToolDto> GetToolDefinitions()
    {
        return _registeredTools.Values
            .Select(tool => new OpenAiToolDto
            {
                // OpenAI's API expects the type to be "function" for all registered tools.
                // This informs OpenAI that this object represents a callable function/tool.
                Type = "function",
                Function = tool.FunctionDefinition
            })
            .ToList();
    }

    /// <summary>
    /// Executes a tool call and returns the result.
    /// </summary>
    public async Task<string> ExecuteToolCallAsync(OpenAiToolCallDto toolCall)
    {
        if (toolCall?.Type != "function")
        {
            throw new ArgumentException($"Unsupported tool call type: {toolCall?.Type ?? "null"}");
        }

        if (toolCall.Function?.Name == null)
        {
            throw new ArgumentException("Tool call function name cannot be null");
        }

        var functionName = toolCall.Function.Name;
        if (!_registeredTools.TryGetValue(functionName, out var registeredTool))
        {
            throw new ArgumentException($"Tool '{functionName}' is not registered");
        }

        var result = await registeredTool.Handler(toolCall.Function.Arguments ?? string.Empty);
        return result;
    }

    /// <summary>
    /// Executes multiple tool calls in parallel and returns the results.
    /// </summary>
    public async Task<Dictionary<string, string>> ExecuteToolCallsAsync(IEnumerable<OpenAiToolCallDto> toolCalls)
    {
        var tasks = toolCalls.Select(async toolCall =>
        {
            var result = await ExecuteToolCallAsync(toolCall);
            return new { toolCall.Id, Result = result };
        });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(r => r.Id, r => r.Result);
    }

    /// <summary>
    /// Creates tool result messages for injection back into the chat conversation.
    /// </summary>
    // This method is a key part of the OpenAI tool-calling workflow. It takes the results of executed tool calls
    // (as a dictionary mapping tool call IDs to their result strings) and formats them as messages with role "tool".
    // These messages are not instructions to execute a tool, but are instead sent back to OpenAI to report the
    // outcome of tool executions. This enables OpenAI to use the results in subsequent assistant responses or
    // to continue the function-calling loop if needed.
    //
    // Typical workflow:
    //   1. You send a chat completion request to OpenAI, including available tool schemas.
    //   2. OpenAI may respond with an assistant message that contains tool call requests (in the tool_calls field).
    //   3. You extract these tool calls and execute the corresponding C# methods locally.
    //   4. For each executed tool, you collect the result and associate it with the tool call ID.
    //   5. You use this method to create messages with role "tool", each containing:
    //        - Role: Always "tool", indicating this is a tool result (not a user or assistant message).
    //        - Content: The serialized result of the tool execution.
    //        - ToolCallId: The ID of the tool call this result corresponds to (so OpenAI can match results to requests).
    //   6. You send these "tool" messages back to OpenAI as part of the conversation.
    //   7. OpenAI uses these results to generate the next assistant message, which may continue the conversation
    //      or request further tool calls.
    //
    // Note: You should never execute a message with role "tool". These are only for reporting results back to OpenAI.

    public List<OpenAiChatMessageDto> CreateToolResultMessages(Dictionary<string, string> toolResults)
    {
        return toolResults.Select(kvp => new OpenAiChatMessageDto
        {
            Role = "tool",
            Content = kvp.Value,
            ToolCallId = kvp.Key
        }).ToList();
    }

    /// <summary>
    /// Checks if a chat completion response contains tool calls that need to be executed.
    /// </summary>
    public bool HasToolCalls(OpenAiChatCompletionResponseDto response)
    {
        return response?.Choices?.Any(choice =>
            choice.Message?.ToolCalls != null && choice.Message.ToolCalls.Count > 0) == true;
    }

    /// <summary>
    /// Extracts tool calls from a chat completion response.
    /// </summary>
    public List<OpenAiToolCallDto> ExtractToolCalls(OpenAiChatCompletionResponseDto response)
    {
        var toolCalls = new List<OpenAiToolCallDto>();

        if (response?.Choices == null) return toolCalls;

        foreach (var choice in response.Choices)
        {
            if (choice.Message?.ToolCalls != null)
            {
                toolCalls.AddRange(choice.Message.ToolCalls);
            }
        }

        return toolCalls;
    }

    /// <summary>
    /// Gets the names of all registered tools.
    /// </summary>
    public IReadOnlyList<string> GetRegisteredToolNames()
    {
        return _registeredTools.Keys.ToArray();
    }

    /// <summary>
    /// Checks if a specific tool is registered.
    /// </summary>
    public bool IsToolRegistered(string toolName)
    {
        return _registeredTools.ContainsKey(toolName);
    }

    /// <summary>
    /// Unregisters a tool by name.
    /// </summary>
    public bool UnregisterTool(string toolName)
    {
        return _registeredTools.Remove(toolName);
    }

    /// <summary>
    /// Clears all registered tools.
    /// </summary>
    public void ClearAllTools()
    {
        _registeredTools.Clear();
    }

    // Internal class for storing metadata and handler for each registered tool.
    // Each RegisteredTool contains:
    //   - Name: The unique name of the tool (used for lookup).
    //   - FunctionDefinition: The OpenAI-compatible schema describing the tool's parameters and usage.
    //   - Handler: A delegate that takes a JSON string (the tool call arguments) and returns a Task<string> (the serialized result).
    //     This delegate always matches the async signature, regardless of whether the original tool was synchronous or asynchronous.
    //     The handler is responsible for deserializing arguments, invoking the C# method, and serializing the result.
    private class RegisteredTool
    {
        public required string Name { get; init; }
        public required OpenAiFunctionDto FunctionDefinition { get; init; }
        public required Func<string, Task<string>> Handler { get; init; }
    }

    // Note: _registeredTools is not thread-safe. If you register/unregister tools from multiple threads,
    // you must provide your own synchronization.
}
