using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pawKitLib.Ai.OpenAi.Models;

namespace pawKitLib.Ai.OpenAi.Services;

public class OpenAiFileManager
{
    private readonly ILogger<OpenAiFileManager> _logger;
    private readonly OpenAiConfigDto _config;
    private readonly HttpClient _client;

    public OpenAiFileManager(
        ILogger<OpenAiFileManager> logger,
        IOptions<OpenAiConfigDto> options,
        HttpClient client)
    {
        _logger = logger;
        _config = options.Value;
        _client = client;
    }

    public async Task<OpenAiFileDto> UploadAsync(OpenAiFileUploadRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_config.BaseUrl}/files";

            using var form = new MultipartFormDataContent();
            OpenAiMultipartFormDataContentHelper.AddFile(form, request.File);
            OpenAiMultipartFormDataContentHelper.AddDto(form, request);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = form
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            return await OpenAiHttpClientHelper.SendAsync<OpenAiFileDto, OpenAiFileManager>(
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
                message: "Unexpected error during file upload.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }

    public async Task<OpenAiFileListResponseDto> ListAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_config.BaseUrl}/files";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            return await OpenAiHttpClientHelper.SendAsync<OpenAiFileListResponseDto, OpenAiFileManager>(
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
                message: "Unexpected error during file listing.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }

    public async Task<OpenAiFileDto> RetrieveAsync(string fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_config.BaseUrl}/files/{fileId}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            return await OpenAiHttpClientHelper.SendAsync<OpenAiFileDto, OpenAiFileManager>(
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
                message: "Unexpected error during file retrieval.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }

    public async Task<OpenAiFileDeleteResponseDto> DeleteAsync(string fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_config.BaseUrl}/files/{fileId}";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Delete, endpoint);

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            return await OpenAiHttpClientHelper.SendAsync<OpenAiFileDeleteResponseDto, OpenAiFileManager>(
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
                message: "Unexpected error during file deletion.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }

    public async Task<byte[]> DownloadContentAsync(string fileId, CancellationToken cancellationToken = default)
    {
        try
        {
            var endpoint = $"{_config.BaseUrl}/files/{fileId}/content";

            using var httpRequest = new HttpRequestMessage(HttpMethod.Get, endpoint);

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

            return await OpenAiHttpClientHelper.SendForBinaryResponseAsync<OpenAiFileManager>(
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
                message: "Unexpected error during file content download.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }
}
