using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

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
        var registeredTool = new RegisteredTool
        {
            Name = name,
            FunctionDefinition = functionDefinition,
            Handler = (argsJson) =>
            {
                try
                {
                    var args = JsonSerializer.Deserialize<T>(argsJson);
                    if (args == null)
                    {
                        throw new ArgumentException($"Failed to deserialize arguments for tool '{name}'");
                    }

                    var result = handler(args);
                    return Task.FromResult(JsonSerializer.Serialize(result));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing tool {ToolName} with arguments {Arguments}", name, argsJson);
                    throw new AiServiceException(
                        message: $"Tool execution failed for '{name}': {ex.Message}",
                        statusCode: null,
                        rawResponse: null,
                        providerDetails: null,
                        innerException: ex);
                }
            }
        };

        _registeredTools[name] = registeredTool;
        _logger.LogDebug("Registered tool {ToolName}", name);
    }

    /// <summary>
    /// Registers an async C# method as a callable tool for OpenAI.
    /// </summary>
    public void RegisterAsyncTool<T>(string name, Func<T, Task<object>> handler, OpenAiFunctionDto functionDefinition)
    {
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
                    _logger.LogError(ex, "Error executing async tool {ToolName} with arguments {Arguments}", name, argsJson);
                    throw new AiServiceException(
                        message: $"Async tool execution failed for '{name}': {ex.Message}",
                        statusCode: null,
                        rawResponse: null,
                        providerDetails: null,
                        innerException: ex);
                }
            }
        };

        _registeredTools[name] = registeredTool;
        _logger.LogDebug("Registered async tool {ToolName}", name);
    }

    /// <summary>
    /// Gets all registered tools as OpenAI tool definitions for use in chat completion requests.
    /// </summary>
    public List<OpenAiToolDto> GetToolDefinitions()
    {
        return _registeredTools.Values
            .Select(tool => new OpenAiToolDto
            {
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
        if (toolCall.Type != "function")
        {
            throw new ArgumentException($"Unsupported tool call type: {toolCall.Type}");
        }

        var functionName = toolCall.Function.Name;
        if (!_registeredTools.TryGetValue(functionName, out var registeredTool))
        {
            throw new ArgumentException($"Tool '{functionName}' is not registered");
        }

        _logger.LogDebug("Executing tool call {ToolCallId} for function {FunctionName}", toolCall.Id, functionName);

        var result = await registeredTool.Handler(toolCall.Function.Arguments);

        _logger.LogDebug("Tool call {ToolCallId} completed successfully", toolCall.Id);
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
        return response.Choices.Any(choice =>
            choice.Message?.ToolCalls != null && choice.Message.ToolCalls.Count > 0);
    }

    /// <summary>
    /// Extracts tool calls from a chat completion response.
    /// </summary>
    public List<OpenAiToolCallDto> ExtractToolCalls(OpenAiChatCompletionResponseDto response)
    {
        var toolCalls = new List<OpenAiToolCallDto>();

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
        return _registeredTools.Keys.ToList().AsReadOnly();
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
        var removed = _registeredTools.Remove(toolName);
        if (removed)
        {
            _logger.LogDebug("Unregistered tool {ToolName}", toolName);
        }
        return removed;
    }

    /// <summary>
    /// Clears all registered tools.
    /// </summary>
    public void ClearAllTools()
    {
        var count = _registeredTools.Count;
        _registeredTools.Clear();
        _logger.LogDebug("Cleared all {ToolCount} registered tools", count);
    }

    private class RegisteredTool
    {
        public required string Name { get; init; }
        public required OpenAiFunctionDto FunctionDefinition { get; init; }
        public required Func<string, Task<string>> Handler { get; init; }
    }
}
