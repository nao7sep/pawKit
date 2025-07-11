namespace pawKitLib.Ai.Sessions;

/// <summary>
/// Defines a tool that the AI model can invoke.
/// </summary>
/// <param name="FunctionName">The name of the function to be called.</param>
/// <param name="Description">A description of what the function does, used by the model to decide when to call it.</param>
/// <param name="ParametersSchemaJson">A JSON Schema object as a string, describing the function's parameters.</param>
public sealed record ToolDefinition(string FunctionName, string Description, string ParametersSchemaJson);