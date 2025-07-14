using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace pawKitLib.Models;

/// <summary>
/// Base DTO for representing dynamic or extensible JSON objects.
/// Inherit from this class to allow your DTO to capture any unmapped properties from JSON responses.
/// This is especially useful for APIs that evolve frequently or return experimental/optional fields.
/// See 'dto-design-guidelines.md' for conventions and best practices.
/// </summary>
public class DynamicDto
{
    /// <summary>
    /// Captures all properties from a JSON response that are not explicitly defined in the DTO.
    /// The dictionary is always initialized and never null, so you can safely enumerate or query it.
    ///
    /// The value type is <see cref="JsonElement"/>, which preserves the full structure and type information
    /// of the original JSON data. This means ExtraProperties can contain primitives, arrays, or nested objects.
    /// You can inspect, enumerate, or deserialize each JsonElement as needed:
    ///   - Use <c>ValueKind</c> to check if the value is an object, array, string, number, etc.
    ///   - Use <c>EnumerateArray()</c> or <c>EnumerateObject()</c> for collections.
    ///   - Use <c>GetRawText()</c> to get the original JSON string for further deserialization.
    ///
    /// This approach is future-proof and works seamlessly with System.Text.Json.
    /// Never store sensitive data in ExtraProperties, as it is unvalidated and dynamic.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> ExtraProperties { get; set; } = new();
}
