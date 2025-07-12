﻿using System.Collections.Immutable;
using System.Text.Json;
using pawKitLib.Ai.Content;
using pawKitLib.Ai.Providers.OpenAI.Dto;
using pawKitLib.Ai.Requests;
using pawKitLib.Ai.Sessions;
using pawKitLib.Ai.Tools;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Handles mapping between abstract pawKit AI types and OpenAI-specific DTOs.
/// This is a static utility class and is not meant to be instantiated.
/// </summary>
internal static class OpenAiMapper
{
    /// <summary>
    /// Maps an abstract <see cref="AiMessage"/> to the provider-specific <see cref="OpenAiMessage"/>.
    /// </summary>
    public static OpenAiMessage MapMessage(AiMessage message)
    {
        var role = MapRole(message.Role);

        return message.Role switch
        {
            MessageRole.Tool => MapToolResultMessage(message, role),
            MessageRole.Assistant when message.Parts.Any(p => p is ToolCallContentPart) => MapAssistantToolCallMessage(message, role),
            _ => new OpenAiMessage
            {
                Role = role,
                Content = MapContent(message.Parts)
            }
        };
    }

    /// <summary>
    /// Maps the abstract <see cref="MessageRole"/> to the OpenAI-specific role string.
    /// </summary>
    private static string MapRole(MessageRole role) => role switch
    {
        MessageRole.User => OpenAiApiConstants.RoleUser,
        MessageRole.Assistant => OpenAiApiConstants.RoleAssistant,
        MessageRole.Tool => OpenAiApiConstants.RoleTool,
        _ => throw new NotSupportedException($"Message role '{role}' is not supported by the OpenAI provider.")
    };

    /// <summary>
    /// Maps a list of abstract <see cref="IContentPart"/>s to the format required by OpenAI (either a single string or an array of part objects).
    /// </summary>
    private static object MapContent(ImmutableList<IContentPart> parts)
    {
        if (parts.Count == 1 && parts[0] is TextContentPart textPart)
        {
            return textPart.Text;
        }

        return parts.Select<IContentPart, object>(part => part switch
        {
            TextContentPart tp => new OpenAiTextContentPart { Text = tp.Text },
            MediaContentPart { Modality: Modality.Image, Resource: var res } => MapImagePart(res),
            _ => throw new NotSupportedException($"Content part of type '{part.GetType().Name}' with modality '{part.Modality}' is not supported by the OpenAI provider.")
        }).ToArray();
    }

    /// <summary>
    /// Maps a <see cref="ResourceRef"/> to a concrete <see cref="OpenAiImageContentPart"/>, handling data URI formatting.
    /// </summary>
    private static OpenAiImageContentPart MapImagePart(ResourceRef resource)
    {
        return resource.Kind switch
        {
            ResourceKind.RemoteUrl => new OpenAiImageContentPart { ImageUrl = new OpenAiImageUrl(resource.Value) },
            ResourceKind.InlineBase64 => new OpenAiImageContentPart { ImageUrl = new OpenAiImageUrl($"{OpenAiApiConstants.DataUriSchemePrefix}{resource.MimeType ?? OpenAiApiConstants.DefaultImageMimeType}{OpenAiApiConstants.Base64DataUriQualifier}{resource.Value}") },
            _ => throw new NotSupportedException($"The OpenAI Chat Completions API does not support resource kind '{resource.Kind}'. Use RemoteUrl or InlineBase64.")
        };
    }

    /// <summary>
    /// Maps a list of abstract <see cref="ToolDefinition"/>s to the provider-specific <see cref="OpenAiTool"/> format.
    /// </summary>
    public static IReadOnlyList<OpenAiTool>? MapTools(ImmutableList<ToolDefinition> toolDefinitions)
    {
        if (toolDefinitions.IsEmpty)
        {
            return null;
        }

        return toolDefinitions.Select(def => new OpenAiTool
        {
            Function = new OpenAiFunctionDefinition
            {
                Name = def.FunctionName,
                Description = def.Description,
                Parameters = JsonDocument.Parse(def.ParametersSchemaJson).RootElement.Clone()
            }
        }).ToList();
    }

    /// <summary>
    /// Maps the abstract <see cref="ToolChoice"/> to the provider-specific format (either a string or an object).
    /// </summary>
    public static object? MapToolChoice(ToolChoice? toolChoice)
    {
        if (toolChoice is null) return null;

        return toolChoice.Mode switch
        {
            ToolChoiceMode.None => OpenAiApiConstants.ToolChoiceNone,
            ToolChoiceMode.Auto => OpenAiApiConstants.ToolChoiceAuto,
            ToolChoiceMode.Any => OpenAiApiConstants.ToolChoiceRequired,
            ToolChoiceMode.Specific when toolChoice.FunctionName is not null => new OpenAiToolChoice { Function = new OpenAiToolChoiceFunction { Name = toolChoice.FunctionName } },
            _ => null
        };
    }

    /// <summary>
    // Maps the abstract <see cref="ResponseFormat"/> to the provider-specific <see cref="OpenAiResponseFormat"/> object.
    /// </summary>
    public static OpenAiResponseFormat? MapResponseFormat(ResponseFormat? responseFormat) => responseFormat switch
    {
        ResponseFormat.JsonObject => new OpenAiResponseFormat { Type = OpenAiApiConstants.ResponseFormatJsonObject },
        _ => null
    };

    /// <summary>
    /// Maps the provider-specific <see cref="OpenAiResponseMessage"/> to the abstract <see cref="AiMessage"/>.
    /// </summary>
    public static AiMessage MapResponse(OpenAiResponseMessage responseMessage)
    {
        var parts = new List<IContentPart>();

        if (!string.IsNullOrWhiteSpace(responseMessage.Content))
        {
            parts.Add(new TextContentPart(responseMessage.Content));
        }

        if (responseMessage.ToolCalls is { Count: > 0 })
        {
            var toolCalls = responseMessage.ToolCalls.Select(tc => new ToolCall(tc.Id, tc.Function.Name, tc.Function.Arguments)).ToImmutableList();
            parts.Add(new ToolCallContentPart(toolCalls));
        }

        return new AiMessage
        {
            Id = Guid.NewGuid(),
            Role = MapResponseRole(responseMessage.Role),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Parts = parts.ToImmutableList()
        };
    }

    private static OpenAiMessage MapToolResultMessage(AiMessage message, string role)
    {
        var toolResultPart = message.Parts.OfType<ToolResultContentPart>().FirstOrDefault()
                             ?? throw new InvalidOperationException("A message with role 'Tool' must contain a ToolResultContentPart.");
        return new OpenAiMessage
        {
            Role = role,
            ToolCallId = toolResultPart.ToolCallId,
            Content = toolResultPart.Content
        };
    }

    private static OpenAiMessage MapAssistantToolCallMessage(AiMessage message, string role)
    {
        var toolCallParts = message.Parts.OfType<ToolCallContentPart>().SelectMany(p => p.ToolCalls);
        var openAiToolCalls = toolCallParts.Select(tc => new OpenAiToolCall
        {
            Id = tc.Id,
            Function = new OpenAiToolCallFunction { Name = tc.FunctionName, Arguments = tc.ArgumentsJson }
        }).ToList();

        var textContent = string.Join(Environment.NewLine, message.Parts.OfType<TextContentPart>().Select(p => p.Text));

        return new OpenAiMessage
        {
            Role = role,
            Content = string.IsNullOrWhiteSpace(textContent) ? null : textContent,
            ToolCalls = openAiToolCalls
        };
    }

    private static MessageRole MapResponseRole(string role) => role switch
    {
        OpenAiApiConstants.RoleAssistant => MessageRole.Assistant,
        _ => throw new NotSupportedException($"Received an unexpected message role '{role}' from the OpenAI API.")
    };
}