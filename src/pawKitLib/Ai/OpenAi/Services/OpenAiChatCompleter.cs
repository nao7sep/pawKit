using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

public class OpenAiChatCompleter
{
    private readonly ILogger<OpenAiChatCompleter> _logger;
    private readonly OpenAiConfigDto _config;
    private readonly HttpClient _client;

    public OpenAiChatCompleter(
        ILogger<OpenAiChatCompleter> logger,
        IOptions<OpenAiConfigDto> options,
        HttpClient client)
    {
        _logger = logger;
        _config = options.Value;
        _client = client;
    }

    public async Task<OpenAiChatCompletionResponseDto> CompleteAsync(OpenAiChatCompletionRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_config.BaseUrl}/chat/completions";

            var jsonRequest = JsonSerializer.Serialize(request);
            using var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = httpContent
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            return await OpenAiHttpClientHelper.SendAsync<OpenAiChatCompletionResponseDto, OpenAiChatCompleter>(
                _logger,
                _client,
                httpRequest,
                cancellationToken
            );
        }
        catch (AiServiceException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new AiServiceException(
                message: "Unexpected error during chat completion.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }

    public async IAsyncEnumerable<OpenAiChatCompletionStreamChunkDto> CompleteStreamAsync(
        OpenAiChatCompletionRequestDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Add streaming parameter to request
        request.ExtraProperties["stream"] = JsonSerializer.SerializeToElement(true);

        var endpoint = $"{_config.BaseUrl}/chat/completions";
        var jsonRequest = JsonSerializer.Serialize(request);

        using var httpContent = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = httpContent
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

        using var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new AiServiceException(
                message: "OpenAI API request failed during streaming.",
                statusCode: ((int)response.StatusCode).ToString(),
                rawResponse: errorContent,
                providerDetails: null);
        }

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(cancellationToken)) != null)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            // Skip empty lines and comments
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(':'))
                continue;

            // Parse SSE format: "data: {json}"
            if (line.StartsWith("data: "))
            {
                var jsonData = line.Substring(6); // Remove "data: " prefix

                // Check for stream end marker
                if (jsonData.Trim() == "[DONE]")
                    yield break;

                OpenAiChatCompletionStreamChunkDto? chunk = null;
                try
                {
                    chunk = JsonSerializer.Deserialize<OpenAiChatCompletionStreamChunkDto>(jsonData);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning("Failed to parse streaming chunk: {JsonData}. Error: {Error}", jsonData, ex.Message);
                    // Continue processing other chunks instead of failing completely
                    continue;
                }

                if (chunk != null)
                    yield return chunk;
            }
        }
    }
}
