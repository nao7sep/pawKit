namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Represents a request from the model to call a specific tool.
/// </summary>
/// <param name="Id">A unique identifier for this tool call, required to associate it with a result.</param>
/// <param name="FunctionName">The name of the function to be called.</param>
/// <param name="ArgumentsJson">The arguments to the function, provided as a JSON string.</param>
public sealed record ToolCall(string Id, string FunctionName, string ArgumentsJson);