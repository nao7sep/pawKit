using System.ComponentModel;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Attribute for marking methods as OpenAI tools with descriptions.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
public class OpenAiToolAttribute : DescriptionAttribute
{
    public OpenAiToolAttribute(string description) : base(description)
    {
    }
}
