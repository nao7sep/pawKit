﻿using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using pawKitLib.Ai.Abstractions;
using pawKitLib.Ai.Providers.OpenAI.Dto;
using pawKitLib.Ai.Requests;
using pawKitLib.Ai.Sessions;

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
    public async Task<IReadOnlyList<AiMessage>> GetCompletionAsync(AiRequestContext context, InferenceParameters parameters, CancellationToken cancellationToken = default)
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

        return responseDto.Choices
            .Select(choice => OpenAiMapper.MapResponse(choice.Message))
            .ToList();
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
            messages.Add(new OpenAiMessage { Role = OpenAiApiConstants.RoleSystem, Content = context.SystemPrompt });
        }

        foreach (var message in context.Messages)
        {
            messages.Add(OpenAiMapper.MapMessage(message));
        }

        var tools = OpenAiMapper.MapTools(context.AvailableTools);
        var toolChoice = OpenAiMapper.MapToolChoice(parameters.ToolChoice);
        var responseFormat = OpenAiMapper.MapResponseFormat(parameters.ResponseFormat);

        // The OpenAI API expects float values for logit_bias. The abstract InferenceParameters
        // uses int, so we must perform a conversion here.
        var logitBias = parameters.LogitBias?.ToDictionary(
            kvp => kvp.Key,
            kvp => (float)kvp.Value);

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
            User = parameters.UserId,
            N = parameters.N,
            LogProbs = parameters.LogProbs,
            TopLogProbs = parameters.TopLogProbs
        };
    }

    /// <summary>
    /// Deserializes an error response and throws a structured <see cref="OpenAiApiException"/>.
    /// </summary>
    private static async Task HandleErrorResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        string errorMessage = $"OpenAI API request failed with status code {(int)response.StatusCode}: {response.ReasonPhrase}";
        string? errorType = null;

        OpenAiError? errorDetails = null;
        try
        {
            var errorDto = JsonSerializer.Deserialize<OpenAiErrorResponse>(errorContent);
            errorDetails = errorDto?.Error;
            if (errorDetails is not null)
            {
                errorMessage = $"OpenAI API Error: {errorDetails.Message} (Type: {errorDetails.Type}, Code: {errorDetails.Code})";
                errorType = errorDetails.Type;
            }
        }
        catch (JsonException) {
            errorMessage = $"OpenAI API request failed with status code {(int)response.StatusCode}. The error response body was not valid JSON. Body: {errorContent}";
        }

        throw new OpenAiApiException(errorMessage, (int)response.StatusCode, errorType);
    }
}