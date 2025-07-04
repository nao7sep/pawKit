using System.Collections;

namespace PawKitLib.Logging.Structured;

/// <summary>
/// Represents the state for structured logging with message template and properties.
/// </summary>
public sealed class StructuredLogState : IReadOnlyList<KeyValuePair<string, object?>>
{
    private readonly string _messageTemplate;
    private readonly object?[] _args;
    private readonly MessageTemplateParser.ParseResult _parseResult;

    /// <summary>
    /// Gets the formatter function for structured log states.
    /// </summary>
    public static readonly Func<StructuredLogState, Exception?, string> Formatter = (state, exception) => state.ToString();

    /// <summary>
    /// Gets the message template.
    /// </summary>
    public string MessageTemplate => _messageTemplate;

    /// <summary>
    /// Gets the arguments.
    /// </summary>
    public object?[] Args => _args;

    /// <summary>
    /// Gets the structured properties extracted from the template and arguments.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Properties => _parseResult.Properties;

    /// <summary>
    /// Gets the formatted message.
    /// </summary>
    public string FormattedMessage => _parseResult.FormattedMessage;

    /// <summary>
    /// Gets the number of properties.
    /// </summary>
    public int Count => _parseResult.Properties.Count;

    /// <summary>
    /// Gets the property at the specified index.
    /// </summary>
    /// <param name="index">The index of the property.</param>
    /// <returns>The property key-value pair.</returns>
    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _parseResult.Properties.ElementAt(index);
        }
    }

    /// <summary>
    /// Initializes a new instance of the StructuredLogState class.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    public StructuredLogState(string messageTemplate, object?[] args)
    {
        _messageTemplate = messageTemplate ?? throw new ArgumentNullException(nameof(messageTemplate));
        _args = args ?? throw new ArgumentNullException(nameof(args));
        _parseResult = MessageTemplateParser.Parse(_messageTemplate, _args);
    }

    /// <summary>
    /// Returns the formatted message.
    /// </summary>
    /// <returns>The formatted message string.</returns>
    public override string ToString()
    {
        return _parseResult.FormattedMessage;
    }

    /// <summary>
    /// Gets an enumerator for the properties.
    /// </summary>
    /// <returns>An enumerator for the properties.</returns>
    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        return _parseResult.Properties.GetEnumerator();
    }

    /// <summary>
    /// Gets an enumerator for the properties.
    /// </summary>
    /// <returns>An enumerator for the properties.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}