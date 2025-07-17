using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

public class OpenAiImageGenerator
{
    private readonly ILogger<OpenAiImageGenerator> _logger;
    private readonly OpenAiConfigDto _config;
    private readonly HttpClient _client;

    public OpenAiImageGenerator(
        ILogger<OpenAiImageGenerator> logger,
        IOptions<OpenAiConfigDto> options,
        HttpClient client)
    {
        _logger = logger;
        _config = options.Value;
        _client = client;
    }

    public async Task<OpenAiImageGenerationResponseDto> GenerateImageAsync(OpenAiImageGenerationRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_config.BaseUrl}/images/generations";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(request)
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            return await OpenAiHttpClientHelper.SendAsync<OpenAiImageGenerationResponseDto, OpenAiImageGenerator>(
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
                message: "Unexpected error during image generation.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }
}
