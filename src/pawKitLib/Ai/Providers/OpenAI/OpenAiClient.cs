﻿using System.Collections.Immutable;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using pawKitLib.Ai.Abstractions;
using pawKitLib.Ai.Content;
using pawKitLib.Ai.Requests;
using pawKitLib.Ai.Sessions;
using pawKitLib.Ai.Tools;

namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// An implementation of <see cref="IAiClient"/> for interacting with the OpenAI API.
/// </summary>
public sealed class OpenAiClient : IAiClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenAiClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client for making requests.</param>
    /// <param name="options">The configuration options for the OpenAI client.</param>
    public OpenAiClient(HttpClient httpClient, IOptions<OpenAiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ArgumentException("OpenAI API key is missing.", nameof(options));
        }

        // The HttpClient is intentionally not configured here.
        // As a typed client, its configuration (BaseAddress, DefaultRequestHeaders, etc.)
        // is handled by IHttpClientFactory during dependency injection setup.
        // This approach centralizes configuration and follows modern .NET best practices.

    }

    /// <inheritdoc />
    public async Task<AiMessage> GetCompletionAsync(AiRequestContext context, InferenceParameters parameters, CancellationToken cancellationToken = default)
    {
        var request = BuildRequest(context, parameters);

        var response = await _httpClient.PostAsJsonAsync(OpenAiApiConstants.ChatCompletionsEndpoint, request, cancellationToken).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            await HandleErrorResponseAsync(response, cancellationToken).ConfigureAwait(false);
        }

        var responseDto = await response.Content.ReadFromJsonAsync<OpenAiChatCompletionResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);

        if (responseDto?.Choices is not { Count: > 0 })
        {
            throw new InvalidOperationException("The OpenAI API returned a successful response with no choices.");
        }

        // We only support N=1, so we take the first choice.
        var choice = responseDto.Choices[0];

        return MapResponse(choice.Message);
    }

    /// <summary>
    /// Builds the provider-specific request DTO from the abstract context and parameters.
    /// </summary>
    /// <param name="context">The prepared request context.</param>
    /// <param name="parameters">The inference parameters.</param>
    /// <returns>A fully constructed <see cref="OpenAiChatCompletionRequest"/>.</returns>
    private OpenAiChatCompletionRequest BuildRequest(AiRequestContext context, InferenceParameters parameters)
    {
        if (string.IsNullOrWhiteSpace(parameters.ModelId))
        {
            throw new ArgumentException("ModelId must be specified in InferenceParameters for the OpenAI provider.", nameof(parameters));
        }

        var messages = new List<OpenAiMessage>();

        if (!string.IsNullOrWhiteSpace(context.SystemPrompt))
        {
            messages.Add(new OpenAiMessage { Role = OpenAiApiConstants.RoleSystem, Content = context.SystemPrompt! });
        }

        foreach (var message in context.Messages)
        {
            messages.Add(MapMessage(message));
        }

        var tools = MapTools(context.AvailableTools);
        var toolChoice = MapToolChoice(parameters.ToolChoice);
        var responseFormat = MapResponseFormat(parameters.ResponseFormat);

        return new OpenAiChatCompletionRequest
        {
            Model = parameters.ModelId,
            Messages = messages,
            Temperature = parameters.Temperature,
            MaxTokens = parameters.MaxTokens,
            TopP = parameters.TopP,
            Stop = parameters.StopSequences?.ToList(),
            ResponseFormat = responseFormat,
            Tools = tools,
            ToolChoice = toolChoice,
            FrequencyPenalty = parameters.FrequencyPenalty,
            PresencePenalty = parameters.PresencePenalty,
            Seed = parameters.Seed,
            LogitBias = parameters.LogitBias,
            User = parameters.UserId
        };
    }

    /// <summary>
    /// Maps an abstract <see cref="AiMessage"/> to the provider-specific <see cref="OpenAiMessage"/>.
    /// </summary>
    /// <param name="message">The abstract message.</param>
    /// <returns>The provider-specific message.</returns>
    private static OpenAiMessage MapMessage(AiMessage message)
    {
        var role = MapRole(message.Role);

        if (message.Role == MessageRole.Tool)
        {
            var toolResultPart = message.Parts.OfType<ToolResultContentPart>().FirstOrDefault();
            if (toolResultPart is null)
            {
                throw new InvalidOperationException("A message with role 'Tool' must contain a ToolResultContentPart.");
            }
            return new OpenAiMessage
            {
                Role = role,
                ToolCallId = toolResultPart.ToolCallId,
                Content = toolResultPart.Content
            };
        }

        if (message.Role == MessageRole.Assistant && message.Parts.Any(p => p is ToolCallContentPart))
        {
            var toolCallParts = message.Parts.OfType<ToolCallContentPart>().SelectMany(p => p.ToolCalls);
            var openAiToolCalls = toolCallParts.Select(tc => new OpenAiToolCall
            {
                Id = tc.Id,
                Function = new OpenAiToolCallFunction
                {
                    Name = tc.FunctionName,
                    Arguments = tc.ArgumentsJson
                }
            }).ToList();

            var textContent = string.Join(Environment.NewLine, message.Parts.OfType<TextContentPart>().Select(p => p.Text));

            return new OpenAiMessage
            {
                Role = role,
                Content = string.IsNullOrWhiteSpace(textContent) ? null : textContent,
                ToolCalls = openAiToolCalls
            };
        }

        return new OpenAiMessage
        {
            Role = role,
            Content = MapContent(message.Parts)
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
            ResourceKind.InlineBase64 => new OpenAiImageContentPart { ImageUrl = new OpenAiImageUrl($"data:{resource.MimeType ?? "image/jpeg"};base64,{resource.Value}") },
            _ => throw new NotSupportedException($"The OpenAI Chat Completions API does not support resource kind '{resource.Kind}'. Use RemoteUrl or InlineBase64.")
        };
    }

    /// <summary>
    /// Maps a list of abstract <see cref="ToolDefinition"/>s to the provider-specific <see cref="OpenAiTool"/> format.
    /// </summary>
    private static IReadOnlyList<OpenAiTool>? MapTools(ImmutableList<ToolDefinition> toolDefinitions)
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
    private static object? MapToolChoice(ToolChoice? toolChoice)
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
    private static OpenAiResponseFormat? MapResponseFormat(ResponseFormat? responseFormat) => responseFormat switch
    {
        ResponseFormat.JsonObject => new OpenAiResponseFormat { Type = OpenAiApiConstants.ResponseFormatJsonObject },
        _ => null
    };

    /// <summary>
    /// Maps the provider-specific <see cref="OpenAiResponseMessage"/> to the abstract <see cref="AiMessage"/>.
    /// </summary>
    private static AiMessage MapResponse(OpenAiResponseMessage responseMessage)
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

    /// <summary>
    /// Maps the role string from an OpenAI response to the abstract <see cref="MessageRole"/>.
    /// </summary>
    private static MessageRole MapResponseRole(string role) => role switch
    {
        OpenAiApiConstants.RoleAssistant => MessageRole.Assistant,
        // The API should only return "assistant" roles in this context.
        // Other roles like "user" or "system" would indicate a misunderstanding of the API contract.
        _ => throw new NotSupportedException($"Received an unexpected message role '{role}' from the OpenAI API.")
    };

    /// <summary>
    /// Deserializes an error response and throws a structured <see cref="OpenAiApiException"/>.
    /// </summary>
    private static async Task HandleErrorResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        string errorMessage = $"OpenAI API request failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}";
        string? errorType = null;

        try
        {
            var errorDto = JsonSerializer.Deserialize<OpenAiErrorResponse>(errorContent);
            if (errorDto is not null)
            {
                errorMessage = $"OpenAI API Error: {errorDto.Error.Message} (Type: {errorDto.Error.Type}, Code: {errorDto.Error.Code})";
                errorType = errorDto.Error.Type;
            }
        }
        catch (JsonException) { /* Ignore if the error response isn't valid JSON */ }

        throw new OpenAiApiException(errorMessage, (int)response.StatusCode, errorType);
    }
}