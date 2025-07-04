using System.Text;
using System.Text.RegularExpressions;

namespace PawKitLib.Logging.Structured;

/// <summary>
/// Parses message templates and extracts structured properties.
/// </summary>
public static class MessageTemplateParser
{
    private static readonly Regex PropertyRegex = new(@"\{([^}]+)\}", RegexOptions.Compiled);

    /// <summary>
    /// Parses a message template and arguments to extract structured properties.
    /// </summary>
    /// <param name="messageTemplate">The message template with property placeholders.</param>
    /// <param name="args">The arguments to substitute into the template.</param>
    /// <returns>A result containing the formatted message and extracted properties.</returns>
    public static ParseResult Parse(string messageTemplate, params object?[] args)
    {
        if (string.IsNullOrEmpty(messageTemplate))
        {
            return new ParseResult(messageTemplate ?? string.Empty, new Dictionary<string, object?>());
        }

        var properties = new Dictionary<string, object?>();
        var matches = PropertyRegex.Matches(messageTemplate);

        if (matches.Count == 0)
        {
            return new ParseResult(messageTemplate, properties);
        }

        var formattedMessage = new StringBuilder(messageTemplate);
        var argIndex = 0;
        var offset = 0; // Track position changes due to replacements

        foreach (Match match in matches)
        {
            if (argIndex >= args.Length)
                break;

            var propertyName = match.Groups[1].Value;
            var propertyValue = args[argIndex];

            // Clean property name (remove formatting specifiers)
            var cleanPropertyName = CleanPropertyName(propertyName);

            // Store the property
            if (!properties.ContainsKey(cleanPropertyName))
            {
                properties[cleanPropertyName] = propertyValue;
            }

            // Replace the placeholder with the formatted value
            var formattedValue = FormatValue(propertyValue, propertyName);
            var startIndex = match.Index + offset;
            var length = match.Length;

            formattedMessage.Remove(startIndex, length);
            formattedMessage.Insert(startIndex, formattedValue);

            // Update offset for next replacement
            offset += formattedValue.Length - length;

            argIndex++;
        }

        return new ParseResult(formattedMessage.ToString(), properties);
    }

    /// <summary>
    /// Extracts property names from a message template.
    /// </summary>
    /// <param name="messageTemplate">The message template.</param>
    /// <returns>A list of property names found in the template.</returns>
    public static IList<string> ExtractPropertyNames(string messageTemplate)
    {
        if (string.IsNullOrEmpty(messageTemplate))
            return new List<string>();

        var matches = PropertyRegex.Matches(messageTemplate);
        var propertyNames = new List<string>();

        foreach (Match match in matches)
        {
            var propertyName = CleanPropertyName(match.Groups[1].Value);
            if (!propertyNames.Contains(propertyName))
            {
                propertyNames.Add(propertyName);
            }
        }

        return propertyNames;
    }

    /// <summary>
    /// Cleans a property name by removing formatting specifiers.
    /// </summary>
    /// <param name="propertyName">The raw property name from the template.</param>
    /// <returns>The cleaned property name.</returns>
    private static string CleanPropertyName(string propertyName)
    {
        // Remove formatting specifiers (e.g., "Name:l" becomes "Name")
        var colonIndex = propertyName.IndexOf(':');
        if (colonIndex >= 0)
        {
            return propertyName.Substring(0, colonIndex);
        }

        return propertyName;
    }

    /// <summary>
    /// Formats a value according to any formatting specifiers in the property name.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="propertyName">The property name which may contain formatting specifiers.</param>
    /// <returns>The formatted value as a string.</returns>
    private static string FormatValue(object? value, string propertyName)
    {
        if (value == null)
            return "null";

        // Check for formatting specifiers
        var colonIndex = propertyName.IndexOf(':');
        if (colonIndex >= 0 && colonIndex < propertyName.Length - 1)
        {
            var formatSpecifier = propertyName.Substring(colonIndex + 1);
            return ApplyFormatSpecifier(value, formatSpecifier);
        }

        return value.ToString() ?? "null";
    }

    /// <summary>
    /// Applies a format specifier to a value.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <param name="formatSpecifier">The format specifier.</param>
    /// <returns>The formatted value.</returns>
    private static string ApplyFormatSpecifier(object value, string formatSpecifier)
    {
        try
        {
            return formatSpecifier.ToLowerInvariant() switch
            {
                "l" => value.ToString()?.ToLowerInvariant() ?? "null",
                "u" => value.ToString()?.ToUpperInvariant() ?? "null",
                _ when value is IFormattable formattable => formattable.ToString(formatSpecifier, null),
                _ => value.ToString() ?? "null"
            };
        }
        catch
        {
            // If formatting fails, return the original value
            return value.ToString() ?? "null";
        }
    }

    /// <summary>
    /// Represents the result of parsing a message template.
    /// </summary>
    public sealed class ParseResult
    {
        /// <summary>
        /// Gets the formatted message with placeholders replaced.
        /// </summary>
        public string FormattedMessage { get; }

        /// <summary>
        /// Gets the extracted structured properties.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Properties { get; }

        /// <summary>
        /// Initializes a new instance of the ParseResult class.
        /// </summary>
        /// <param name="formattedMessage">The formatted message.</param>
        /// <param name="properties">The extracted properties.</param>
        public ParseResult(string formattedMessage, IReadOnlyDictionary<string, object?> properties)
        {
            FormattedMessage = formattedMessage;
            Properties = properties;
        }
    }
}