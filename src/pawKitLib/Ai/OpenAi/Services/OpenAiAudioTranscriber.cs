using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

public class OpenAiAudioTranscriber
{
    // The logger is currently used only for logging JSON strings received from the OpenAI API.
    // These logs serve as a temporary sink for diagnostic data, as there is no dedicated destination for such payloads.
    // Logging occurs only if the log level is set to Debug or lower, ensuring minimal impact in production environments.
    //
    // Exceptions are not logged here as they are thrown; in most cases, exceptions should be handled by the callers of this class.
    // If a method both handles and logs exceptions, it assumes more than one responsibility, which is discouraged for maintainable code.
    private readonly ILogger<OpenAiAudioTranscriber> _logger;
    private readonly OpenAiConfigDto _config;
    private readonly HttpClient _client;

    public OpenAiAudioTranscriber(
        ILogger<OpenAiAudioTranscriber> logger,
        OpenAiConfigDto config,
        HttpClient client)
    {
        _logger = logger;
        _config = config;
        _client = client;
    }

    public async Task<OpenAiAudioTranscribeResponseDto> TranscribeAsync(OpenAiAudioTranscribeRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_config.BaseUrl}/audio/transcriptions";

            using var form = new MultipartFormDataContent();
            OpenAiMultipartFormDataContentHelper.AddFile(form, request.File);
            OpenAiMultipartFormDataContentHelper.AddDto(form, request);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = form
            };
            // Set the Authorization header only for this request
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            return await OpenAiHttpClientHelper.SendAsync<OpenAiAudioTranscribeResponseDto, OpenAiAudioTranscriber>(
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
                message: "Unexpected error during audio transcription.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }
}
