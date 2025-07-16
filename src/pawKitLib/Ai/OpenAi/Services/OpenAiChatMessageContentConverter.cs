using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

/// <summary>
/// Handles the polymorphic nature of the 'content' property in OpenAI chat messages,
/// which can be a string, an array of content parts, or null.
/// </summary>
public class OpenAiChatMessageContentConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.StartArray:
                return JsonSerializer.Deserialize<List<OpenAiChatMessageContentPartDto>>(ref reader, options);
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException("Expected a string, an array of content parts, or null for the 'content' property.");
        }
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case string stringValue:
                writer.WriteStringValue(stringValue);
                break;
            case IEnumerable<OpenAiChatMessageContentPartDto> parts:
                JsonSerializer.Serialize(writer, parts, options);
                break;
            case null:
                writer.WriteNullValue();
                break;
            default:
                throw new JsonException($"Unsupported type for 'content' property: {value.GetType().Name}. Must be a string or IEnumerable<OpenAiChatMessageContentPartDto>.");
        }
    }
}
