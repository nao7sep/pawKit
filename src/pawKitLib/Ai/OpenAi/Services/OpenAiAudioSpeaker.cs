using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

public class OpenAiAudioSpeaker
{
    private readonly ILogger<OpenAiAudioSpeaker> _logger;
    private readonly OpenAiConfigDto _config;
    private readonly HttpClient _client;

    public OpenAiAudioSpeaker(
        ILogger<OpenAiAudioSpeaker> logger,
        IOptions<OpenAiConfigDto> options,
        HttpClient client)
    {
        _logger = logger;
        _config = options.Value;
        _client = client;
    }

    public async Task<byte[]> GenerateSpeechAsync(OpenAiAudioSpeechRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_config.BaseUrl}/audio/speech";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                // JsonContent.Create serializes the provided request object (OpenAiAudioSpeechRequestDto) to JSON using System.Text.Json.
                // It creates an HttpContent instance with the serialized JSON as the body and automatically sets the Content-Type header to
                // "application/json; charset=utf-8". This is the recommended way to send strongly-typed .NET objects as JSON in HTTP requests.
                //
                // IMPORTANT: The base class DynamicDto uses the [JsonExtensionData] attribute. This means:
                //   - Any extra properties present in the object (not defined as explicit C# properties) are included in serialization as additional top-level JSON fields.
                //   - The dictionary is "flattened" into the top-level JSON object; keys are not nested under a property like "extraProperties".
                //   - Any unknown fields in the JSON response are captured into the extension data dictionary during deserialization.
                // This allows the DTOs to be forward-compatible with new or unexpected fields from the OpenAI API, and lets you add extra data at runtime if needed.
                //
                // In summary:
                //   - All defined properties are serialized/deserialized as normal.
                //   - Any extra properties (not defined in the DTO) are handled by DynamicDto's extension data dictionary.
                //   - This makes the DTOs robust to API changes and safe for round-tripping unknown fields.
                //
                // When the request is sent, the JSON is included as the POST body, ensuring correct serialization and content type for the OpenAI API.
                Content = JsonContent.Create(request)
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            return await OpenAiHttpClientHelper.SendForBinaryResponseAsync<OpenAiAudioSpeaker>(
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
                "Unexpected error during audio generation.", null, null, null, ex);
        }
    }
}
