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
        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        foreach (var (paramName, paramDesc, isRequired) in parameters)
        {
            properties[paramName] = new
            {
                type = "string",
                description = paramDesc
            };

            if (isRequired)
            {
                required.Add(paramName);
            }
        }

        var schema = new
        {
            type = "object",
            properties = properties,
            required = required.Count > 0 ? required : null
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
            required = required.Count > 0 ? required : null
        };
    }

    /// <summary>
    /// Creates a JSON schema property for a specific .NET type.
    /// </summary>
    private static object CreatePropertySchema(Type type, string description)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType switch
        {
            Type t when t == typeof(string) => new
            {
                type = "string",
                description = description
            },
            Type t when t == typeof(int) || t == typeof(long) || t == typeof(short) || t == typeof(byte) => new
            {
                type = "integer",
                description = description
            },
            Type t when t == typeof(double) || t == typeof(float) || t == typeof(decimal) => new
            {
                type = "number",
                description = description
            },
            Type t when t == typeof(bool) => new
            {
                type = "boolean",
                description = description
            },
            Type t when t.IsEnum => new
            {
                type = "string",
                @enum = Enum.GetNames(t),
                description = description
            },
            Type t when t.IsArray => new
            {
                type = "array",
                items = CreatePropertySchema(t.GetElementType()!, string.Empty),
                description = description
            },
            Type t when IsListType(t) => new
            {
                type = "array",
                items = CreatePropertySchema(t.GetGenericArguments()[0], string.Empty),
                description = description
            },
            _ => new
            {
                type = "object",
                description = description
            }
        };
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
        return type.IsGenericType &&
               (type.GetGenericTypeDefinition() == typeof(List<>) ||
                type.GetGenericTypeDefinition() == typeof(IList<>) ||
                type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>)));
    }
}

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
