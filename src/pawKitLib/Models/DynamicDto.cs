using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace pawKitLib.Models;

/// <summary>
/// Base DTO for representing dynamic or extensible JSON objects. Use as a foundation for other DTOs.
/// See 'dto-design-guidelines.md' for conventions.
/// </summary>
public class DynamicDto
{
    /// <summary>
    /// Stores any properties from an API response not explicitly defined in the DTO.
    /// Always initialized to an empty dictionary. Use for extensibility and evolving APIs.
    /// Do not store sensitive data here.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object?> ExtraProperties { get; set; } = new();
}
