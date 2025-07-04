using System.Collections.Concurrent;
using System.Reflection;

namespace PawKitLib.Logging.Structured;

/// <summary>
/// Manages logging scopes and their associated properties.
/// </summary>
public sealed class LogScope : IDisposable
{
    private static readonly AsyncLocal<LogScope?> _current = new();
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _typePropertyCache = new();
    private readonly LogScope? _parent;
    private readonly Dictionary<string, object?> _properties;
    private bool _disposed;

    /// <summary>
    /// Gets the current active scope.
    /// </summary>
    public static LogScope? Current => _current.Value;

    /// <summary>
    /// Gets the properties associated with this scope.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Properties => _properties;

    /// <summary>
    /// Initializes a new instance of the LogScope class.
    /// </summary>
    /// <param name="state">The state object for this scope.</param>
    private LogScope(object? state)
    {
        _parent = _current.Value;
        _properties = new Dictionary<string, object?>();

        // Extract properties from the state
        ExtractPropertiesFromState(state);

        // Set this as the current scope
        _current.Value = this;
    }

    /// <summary>
    /// Begins a new logging scope.
    /// </summary>
    /// <typeparam name="TState">The type of the state object.</typeparam>
    /// <param name="state">The state object for the scope.</param>
    /// <returns>A disposable scope object.</returns>
    public static IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return new LogScope(state);
    }

    /// <summary>
    /// Gets all properties from the current scope chain.
    /// </summary>
    /// <returns>A dictionary containing all scoped properties.</returns>
    public static IReadOnlyDictionary<string, object?> GetAllScopeProperties()
    {
        var allProperties = new Dictionary<string, object?>();
        var currentScope = Current;

        // Walk up the scope chain and collect properties
        while (currentScope != null)
        {
            foreach (var kvp in currentScope.Properties)
            {
                // Properties from inner scopes take precedence
                if (!allProperties.ContainsKey(kvp.Key))
                {
                    allProperties[kvp.Key] = kvp.Value;
                }
            }
            currentScope = currentScope._parent;
        }

        return allProperties;
    }

    /// <summary>
    /// Extracts properties from a state object.
    /// </summary>
    /// <param name="state">The state object.</param>
    private void ExtractPropertiesFromState(object? state)
    {
        if (state == null)
            return;

        switch (state)
        {
            case IDictionary<string, object?> dictionary:
                foreach (var kvp in dictionary)
                {
                    _properties[kvp.Key] = kvp.Value;
                }
                break;

            case IEnumerable<KeyValuePair<string, object?>> keyValuePairs:
                foreach (var kvp in keyValuePairs)
                {
                    _properties[kvp.Key] = kvp.Value;
                }
                break;

            case string stringState:
                _properties["Scope"] = stringState;
                break;

            default:
                // For other types, try to extract properties using reflection
                ExtractPropertiesUsingReflection(state);
                break;
        }
    }

    /// <summary>
    /// Extracts properties from an object using reflection.
    /// </summary>
    /// <param name="state">The state object.</param>
    private void ExtractPropertiesUsingReflection(object state)
    {
        try
        {
            var type = state.GetType();

            // Use cached properties for better performance
            var properties = _typePropertyCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                 .ToArray());

            foreach (var property in properties)
            {
                try
                {
                    var value = property.GetValue(state);
                    _properties[property.Name] = value;
                }
                catch
                {
                    // Ignore properties that can't be read
                }
            }
        }
        catch
        {
            // If reflection fails, store the object as-is
            _properties["State"] = state;
        }
    }

    /// <summary>
    /// Disposes the scope and restores the previous scope.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _current.Value = _parent;
        _disposed = true;
    }
}