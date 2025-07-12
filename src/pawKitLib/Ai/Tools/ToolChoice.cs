namespace pawKitLib.Ai.Tools;

/// <summary>
/// Specifies a constraint on the model's tool-use behavior for a single request.
/// </summary>
public sealed record ToolChoice
{
    private ToolChoice(ToolChoiceMode mode, string? functionName = null)
    {
        if (mode == ToolChoiceMode.Specific && string.IsNullOrWhiteSpace(functionName))
        {
            throw new ArgumentException("FunctionName must be provided when mode is Specific.", nameof(functionName));
        }
        Mode = mode;
        FunctionName = functionName;
    }

    /// <summary>Gets the mode for tool selection.</summary>
    public ToolChoiceMode Mode { get; }

    /// <summary>Gets the name of the specific function to be called, if <see cref="Mode"/> is <see cref="ToolChoiceMode.Specific"/>.</summary>
    public string? FunctionName { get; }

    /// <summary>The model can choose whether to call a tool.</summary>
    public static ToolChoice Auto() => new(ToolChoiceMode.Auto);
    /// <summary>The model must not call any tool.</summary>
    public static ToolChoice None() => new(ToolChoiceMode.None);
    /// <summary>The model must call at least one tool.</summary>
    public static ToolChoice Any() => new(ToolChoiceMode.Any);
    /// <summary>The model must call the specified tool.</summary>
    public static ToolChoice Specific(string functionName) => new(ToolChoiceMode.Specific, functionName);
}