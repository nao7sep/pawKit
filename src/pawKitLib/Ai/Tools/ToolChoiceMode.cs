namespace pawKitLib.Ai.Tools;

/// <summary>
/// Defines the modes for constraining tool use in an AI request.
/// </summary>
public enum ToolChoiceMode
{
    /// <summary>The model decides whether to call a tool (default behavior).</summary>
    Auto,
    /// <summary>The model is forbidden from calling any tool.</summary>
    None,
    /// <summary>The model is required to call at least one tool.</summary>
    Any,
    /// <summary>The model is required to call a specific tool by its function name.</summary>
    Specific
}