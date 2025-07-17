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
        _registeredTools[name] = CreateRegisteredTool(name, functionDefinition,
            argsJson => Task.FromResult(ExecuteHandler(name, argsJson, handler)));
    }

    /// <summary>
    /// Registers an async C# method as a callable tool for OpenAI.
    /// </summary>
    public void RegisterAsyncTool<T>(string name, Func<T, Task<object>> handler, OpenAiFunctionDto functionDefinition)
    {
        _registeredTools[name] = CreateRegisteredTool(name, functionDefinition,
            argsJson => ExecuteAsyncHandler(name, argsJson, handler));
    }

    /// <summary>
    /// Creates a registered tool with common configuration.
    /// </summary>
    private static RegisteredTool CreateRegisteredTool(string name, OpenAiFunctionDto functionDefinition, Func<string, Task<string>> handler)
    {
        return new RegisteredTool
        {
            Name = name,
            FunctionDefinition = functionDefinition,
            Handler = handler
        };
    }

    /// <summary>
    /// Executes a synchronous handler with error handling.
    /// </summary>
    private static string ExecuteHandler<T>(string name, string argsJson, Func<T, object> handler)
    {
        try
        {
            var args = DeserializeArguments<T>(name, argsJson);
            var result = handler(args);
            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            throw CreateToolExecutionException(name, "Tool execution failed", ex);
        }
    }

    /// <summary>
    /// Executes an asynchronous handler with error handling.
    /// </summary>
    private static async Task<string> ExecuteAsyncHandler<T>(string name, string argsJson, Func<T, Task<object>> handler)
    {
        try
        {
            var args = DeserializeArguments<T>(name, argsJson);
            var result = await handler(args);
            return JsonSerializer.Serialize(result);
        }
        catch (Exception ex)
        {
            throw CreateToolExecutionException(name, "Async tool execution failed", ex);
        }
    }

    /// <summary>
    /// Deserializes tool arguments with validation.
    /// </summary>
    private static T DeserializeArguments<T>(string toolName, string argsJson)
    {
        var args = JsonSerializer.Deserialize<T>(argsJson);
        return args ?? throw new ArgumentException($"Failed to deserialize arguments for tool '{toolName}'");
    }

    /// <summary>
    /// Creates a standardized tool execution exception.
    /// </summary>
    private static AiServiceException CreateToolExecutionException(string toolName, string message, Exception innerException)
    {
        return new AiServiceException(
            message: $"{message} for '{toolName}'",
            statusCode: null,
            rawResponse: null,
            providerDetails: null,
            innerException: innerException);
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

        var result = await registeredTool.Handler(toolCall.Function.Arguments);
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
        return response.Choices.Any(choice => choice.Message?.ToolCalls?.Count > 0);
    }

    /// <summary>
    /// Extracts tool calls from a chat completion response.
    /// </summary>
    public List<OpenAiToolCallDto> ExtractToolCalls(OpenAiChatCompletionResponseDto response)
    {
        return response.Choices
            .Where(choice => choice.Message?.ToolCalls != null)
            .SelectMany(choice => choice.Message!.ToolCalls!)
            .ToList();
    }

    /// <summary>
    /// Gets the names of all registered tools.
    /// </summary>
    public IReadOnlyCollection<string> GetRegisteredToolNames()
    {
        return _registeredTools.Keys;
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
        return removed;
    }

    /// <summary>
    /// Clears all registered tools.
    /// </summary>
    public void ClearAllTools()
    {
        _registeredTools.Clear();
    }

    private class RegisteredTool
    {
        public required string Name { get; init; }
        public required OpenAiFunctionDto FunctionDefinition { get; init; }
        public required Func<string, Task<string>> Handler { get; init; }
    }
}
