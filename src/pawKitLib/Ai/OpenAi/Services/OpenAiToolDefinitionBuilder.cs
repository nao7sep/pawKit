using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

/// <summary>
/// Helper service for building OpenAI tool definitions from C# methods using reflection and attributes.
/// Provides convenient ways to create function schemas for tool calling.
/// </summary>
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
    private static bool IsNullableType(Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    /// <summary>
    /// Checks if a type is a List<T> or IList<T>.
    /// </summary>
    private static bool IsListType(Type type)
    {
        if (!type.IsGenericType) return false;

        var genericTypeDefinition = type.GetGenericTypeDefinition();
        return genericTypeDefinition == typeof(List<>) ||
               genericTypeDefinition == typeof(IList<>) ||
               type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
    }
}
