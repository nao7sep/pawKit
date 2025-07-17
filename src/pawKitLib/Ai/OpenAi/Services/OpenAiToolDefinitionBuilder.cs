using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

// OpenAiToolDefinitionBuilder: Utility for generating OpenAI function schemas from C#
// -------------------------------------------------------------------------
// This static helper class provides methods to create OpenAI-compatible function definitions (schemas)
// from C# methods, types, or manual specifications. It is used to:
//   - Reflect over C# methods and generate JSON schema objects for their parameters.
//   - Use attributes (like DescriptionAttribute) to enrich function and parameter descriptions.
//   - Build function definitions for registering tools with OpenAI's function calling API.
//   - Support both automatic (reflection-based) and manual schema creation.
//
// This class is typically used before registering a tool, to generate the schema that describes
// the tool's name, description, and parameter structure in a way OpenAI understands.

/// <summary>
/// Helper service for building OpenAI tool definitions from C# methods using reflection and attributes.
/// Provides convenient ways to create function schemas for tool calling.
/// </summary>
// This class is stateless and thread-safe. All methods are static and can be used from anywhere
// without instantiation or concern for shared state.
public static class OpenAiToolDefinitionBuilder
{
    /// <summary>
    /// Creates a function definition from a method using reflection.
    /// Uses DescriptionAttribute for function and parameter descriptions.
    /// </summary>
    public static OpenAiFunctionDto CreateFromMethod(MethodInfo method, string? customName = null)
    {
        if (method == null)
            throw new ArgumentNullException(nameof(method));

        var functionName = customName ?? method.Name;
        var description = method.GetCustomAttribute<DescriptionAttribute>()?.Description;

        var parameters = CreateParametersSchema(method);

        return new OpenAiFunctionDto
        {
            Name = functionName,
            Description = description,
            Parameters = parameters
        };
    }

    /// <summary>
    /// Creates a function definition with manual parameter specification.
    /// </summary>
    // This method is for advanced/manual scenarios where you want to provide a custom schema object
    // (including non-string types, nested objects, etc.) directly. Use this if you need full control
    // over the OpenAI function schema, beyond what reflection or the simple helper provides.
    public static OpenAiFunctionDto CreateFunction(
        string name,
        string? description = null,
        object? parameters = null,
        bool? strict = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Function name cannot be null or empty", nameof(name));

        return new OpenAiFunctionDto
        {
            Name = name,
            Description = description,
            Parameters = parameters,
            Strict = strict
        };
    }

    /// <summary>
    /// Creates a simple function definition with basic string parameters.
    /// </summary>
    // This helper is intended for quickly defining an OpenAI function schema where all parameters are simple strings.
    // It is useful when you want to register a tool for OpenAI to call, but you do not want to define a concrete C# method
    // or parameter class in your code. Instead, you specify the function's name, description, and a list of parameters
    // (each with a name, description, and required flag) directly in code.
    //
    // All parameters defined with this method will be treated as strings in the schema, regardless of their intended use.
    // This is ideal for simple, ad-hoc tools or when you want to expose a function to OpenAI without the overhead of
    // creating a full C# type or using reflection.
    //
    // Note:
    // - If you need to support parameters of other types (integer, number, boolean, array, object, enum, etc.),
    //   use CreateFromMethod (for reflection-based schema generation) or CreateFunction (for manual, advanced schemas).
    // - The handler you register for this tool can be a lambda or any C# function that matches the expected signature.
    // - This approach is similar to defining an "anonymous" or "lambda-like" function for OpenAI, where the schema
    //   is defined on the fly and not tied to a concrete method or class in your codebase.
    //
    // Example usage:
    //   var schema = OpenAiToolDefinitionBuilder.CreateSimpleFunction(
    //       "echo",
    //       "Echoes the input string",
    //       ("input", "The string to echo", true)
    //   );
    //   handler: input => input.ToUpper()
    //
    // This will create a function schema with a single required string parameter "input".
    public static OpenAiFunctionDto CreateSimpleFunction(
        string name,
        string? description = null,
        params (string name, string description, bool required)[] parameters)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Function name cannot be null or empty", nameof(name));

        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        if (parameters != null)
        {
            foreach (var (paramName, paramDesc, isRequired) in parameters)
            {
                if (string.IsNullOrWhiteSpace(paramName))
                    continue; // Skip invalid parameter names

                properties[paramName] = new
                {
                    type = "string",
                    description = paramDesc ?? string.Empty
                };

                if (isRequired)
                {
                    required.Add(paramName);
                }
            }
        }

        var schema = new
        {
            type = "object",
            properties = properties,
            required = required.Count > 0 ? required.ToArray() : null
        };

        return new OpenAiFunctionDto
        {
            Name = name,
            Description = description,
            Parameters = schema
        };
    }

    /// <summary>
    /// Creates a JSON schema object for method parameters using reflection.
    /// </summary>
    // This helper is used internally by CreateFromMethod to generate a JSON schema for a C# method's parameters.
    // It inspects the method's parameters using reflection, determines their types, and builds a schema that
    // accurately describes each parameter in a way OpenAI's function calling API can understand.
    //
    // The resulting schema supports all standard JSON types (string, integer, number, boolean, array, object, enum).
    // Parameter descriptions are extracted from DescriptionAttribute if present.
    // Required parameters are determined by checking for default values and nullability.
    //
    // This method is not intended for direct use—it's part of the reflection-based workflow for exposing
    // existing C# methods as OpenAI-callable tools via CreateFromMethod.
    private static object CreateParametersSchema(MethodInfo method)
    {
        var parameters = method.GetParameters();
        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        foreach (var param in parameters)
        {
            var paramName = param.Name ?? "unknown";
            var paramType = param.ParameterType;
            var description = param.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;

            // Determine if parameter is required (not nullable and no default value)
            var isRequired = !param.HasDefaultValue && !IsNullableType(paramType);

            if (isRequired)
            {
                required.Add(paramName);
            }

            // Create property schema based on type
            properties[paramName] = CreatePropertySchema(paramType, description);
        }

        return new
        {
            type = "object",
            properties = properties,
            required = required.Count > 0 ? required.ToArray() : null
        };
    }

    /// <summary>
    /// Creates a JSON schema property for a specific .NET type.
    /// </summary>
    // This internal helper maps a .NET type to a JSON schema property definition compatible with OpenAI's function calling API.
    // It is used by the schema builder to describe each parameter's type and constraints in a way OpenAI understands.
    //
    // Supported mappings:
    //   - string:      { type = "string" }
    //   - integer:     { type = "integer" } (int, long, short, byte)
    //   - number:      { type = "number" } (double, float, decimal)
    //   - boolean:     { type = "boolean" }
    //   - enum:        { type = "string", enum = [...] }
    //   - array:       { type = "array", items = ... } (for arrays and generic lists)
    //   - object:      { type = "object" } (fallback for complex or unknown types)
    //
    // Limitations:
    //   - For custom classes and dictionaries, this method does NOT recursively describe nested properties;
    //     it simply outputs { type = "object" } with no further detail.
    //   - Advanced JSON Schema features (e.g., oneOf, anyOf, allOf, pattern, min/max constraints, nullable types in schema)
    //     are NOT supported.
    //   - Nullability is only used to determine if a parameter is required, not reflected in the schema type.
    //
    // In most real-world scenarios, this covers the vast majority of method parameter types.
    // For deeply nested or highly customized schemas, consider building the schema manually.
    private static object CreatePropertySchema(Type type, string description)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        if (underlyingType == typeof(string))
        {
            return new { type = "string", description };
        }

        if (underlyingType == typeof(int) || underlyingType == typeof(long) ||
            underlyingType == typeof(short) || underlyingType == typeof(byte))
        {
            return new { type = "integer", description };
        }

        if (underlyingType == typeof(double) || underlyingType == typeof(float) ||
            underlyingType == typeof(decimal))
        {
            return new { type = "number", description };
        }

        if (underlyingType == typeof(bool))
        {
            return new { type = "boolean", description };
        }

        if (underlyingType.IsEnum)
        {
            return new { type = "string", @enum = Enum.GetNames(underlyingType), description };
        }

        if (underlyingType.IsArray)
        {
            var elementType = underlyingType.GetElementType();
            if (elementType == null)
            {
                return new { type = "array", description };
            }

            return new
            {
                type = "array",
                items = CreatePropertySchema(elementType, string.Empty),
                description
            };
        }

        if (IsListType(underlyingType))
        {
            return new
            {
                type = "array",
                items = CreatePropertySchema(underlyingType.GetGenericArguments()[0], string.Empty),
                description
            };
        }

        return new { type = "object", description };
    }

    /// <summary>
    /// Checks if a type is nullable.
    /// </summary>
    // Used internally to determine if a parameter should be marked as required in the schema.
    private static bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    /// <summary>
    /// Checks if a type is a List<T> or IList<T>.
    /// </summary>
    // Used internally to support array/list parameter types in schema generation.
    private static bool IsListType(Type type)
    {
        if (!type.IsGenericType) return false;

        var genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(List<>) ||
               genericTypeDefinition == typeof(IList<>) ||
               type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
    }
}
