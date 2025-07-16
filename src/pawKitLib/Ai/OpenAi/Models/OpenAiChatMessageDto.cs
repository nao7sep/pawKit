﻿using System.Text.Json.Serialization;
using pawKitLib.Ai.OpenAi.Services;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;
/// <summary>
/// Represents a single message in a chat conversation, used for both requests and responses.
/// Supports simple text content as a string or complex multi-part content (e.g., text and images) as a list.
/// </summary>
public class OpenAiChatMessageDto : DynamicDto
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// The content of the message. This can be a simple string for text-only messages,
    /// or a list of <see cref="OpenAiChatMessageContentPartDto"/> for multi-modal messages
    /// (e.g., including images). The custom converter handles serialization and deserialization.
    /// In responses, this is typically a string. Can be null in responses that use tool calls.
    /// </summary>
    [JsonPropertyName("content")]
    [JsonConverter(typeof(OpenAiChatMessageContentConverter))]
    public object? Content { get; set; }

    [JsonPropertyName("audio")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public OpenAiAudioDto? Audio { get; set; }

    [JsonPropertyName("tool_calls")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OpenAiToolCallDto>? ToolCalls { get; set; }

    [JsonPropertyName("tool_call_id")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ToolCallId { get; set; }
}
