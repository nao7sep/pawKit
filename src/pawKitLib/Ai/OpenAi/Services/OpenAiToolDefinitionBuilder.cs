using System.Collections.Concurrent;
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
    private static readonly ConcurrentDictionary<Type, object> _typeSchemaCache = new();
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
        var properties = new Dictionary<string, object>(parameters.Length);
        List<string>? required = null;

        foreach (var (paramName, paramDesc, isRequired) in parameters)
        {
            properties[paramName] = new
            {
                type = "string",
                description = paramDesc
            };

            if (isRequired)
            {
                required ??= new List<string>();
                required.Add(paramName);
            }
        }

        var schema = new
        {
            type = "object",
            properties,
            required
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
        var properties = new Dictionary<string, object>(parameters.Length);
        List<string>? required = null;

        foreach (var param in parameters)
        {
            var paramName = param.Name ?? "unknown";
            var paramType = param.ParameterType;
            var description = param.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty;

            // Determine if parameter is required (not nullable and no default value)
            var isRequired = !param.HasDefaultValue && !IsNullableType(paramType);

            if (isRequired)
            {
                required ??= new List<string>();
                required.Add(paramName);
            }

            // Create property schema based on type
            properties[paramName] = CreatePropertySchema(paramType, description);
        }

        return new
        {
            type = "object",
            properties,
            required
        };
    }

    /// <summary>
    /// Creates a JSON schema property for a specific .NET type.
    /// </summary>
    private static object CreatePropertySchema(Type type, string description)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        // Use cached schema for base type, then add description
        var baseSchema = _typeSchemaCache.GetOrAdd(underlyingType, CreateBaseTypeSchema);

        // If description is empty, return cached schema directly
        if (string.IsNullOrEmpty(description))
        {
            return baseSchema;
        }

        // Create new schema with description
        return underlyingType switch
        {
            Type t when t == typeof(string) => new { type = "string", description },
            Type t when IsIntegerType(t) => new { type = "integer", description },
            Type t when IsNumberType(t) => new { type = "number", description },
            Type t when t == typeof(bool) => new { type = "boolean", description },
            Type t when t.IsEnum => new { type = "string", @enum = Enum.GetNames(t), description },
            Type t when t.IsArray => new
            {
                type = "array",
                items = CreatePropertySchema(t.GetElementType()!, string.Empty),
                description
            },
            Type t when IsListType(t) => new
            {
                type = "array",
                items = CreatePropertySchema(t.GetGenericArguments()[0], string.Empty),
                description
            },
            _ => new { type = "object", description }
        };
    }

    /// <summary>
    /// Creates base type schema without description for caching.
    /// </summary>
    private static object CreateBaseTypeSchema(Type type)
    {
        return type switch
        {
            Type t when t == typeof(string) => new { type = "string" },
            Type t when IsIntegerType(t) => new { type = "integer" },
            Type t when IsNumberType(t) => new { type = "number" },
            Type t when t == typeof(bool) => new { type = "boolean" },
            Type t when t.IsEnum => new { type = "string", @enum = Enum.GetNames(t) },
            Type t when t.IsArray => new
            {
                type = "array",
                items = CreatePropertySchema(t.GetElementType()!, string.Empty)
            },
            Type t when IsListType(t) => new
            {
                type = "array",
                items = CreatePropertySchema(t.GetGenericArguments()[0], string.Empty)
            },
            _ => new { type = "object" }
        };
    }

    /// <summary>
    /// Checks if a type is an integer type.
    /// </summary>
    private static bool IsIntegerType(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte);
    }

    /// <summary>
    /// Checks if a type is a number type.
    /// </summary>
    private static bool IsNumberType(Type type)
    {
        return type == typeof(double) || type == typeof(float) || type == typeof(decimal);
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
