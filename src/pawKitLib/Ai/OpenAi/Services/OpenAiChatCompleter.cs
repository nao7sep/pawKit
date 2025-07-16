using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using pawKitLib.Ai.OpenAi.Models;
using pawKitLib.Conversion;

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

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(request)
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

    // The [EnumeratorCancellation] attribute allows the CancellationToken parameter to be used for cooperative cancellation
    // of the async iterator when consuming this method with 'await foreach'. Without this attribute, cancellation requests
    // from the consumer's CancellationToken would not be properly propagated to the enumerator, potentially causing the
    // iteration to continue even after cancellation is requested. This attribute ensures that cancellation is respected
    // throughout the streaming process, making the method more responsive and robust in cancellation scenarios.
    public async IAsyncEnumerable<OpenAiChatCompletionStreamChunkDto> CompleteStreamAsync(
        OpenAiChatCompletionRequestDto request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var endpoint = $"{_config.BaseUrl}/chat/completions";

        HttpResponseMessage? response = null;
        Stream? stream = null;
        StreamReader? reader = null;

        try
        {
            // Exception handling in this method is designed for the widest possible coverage:
            // Any error during the HTTP request, response header processing, or stream reading will result in an exception being thrown.
            // Because this is an async iterator, exceptions may be thrown not only at the initial call,
            // but also during iteration (i.e., within 'await foreach').
            // It is the caller's responsibility to handle exceptions around the entire 'await foreach' loop to ensure robust error handling.
            try
            {
                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = JsonContent.Create(request)
                };

                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _config.ApiKey);

                // HttpCompletionOption.ResponseHeadersRead instructs HttpClient to return as soon as the response headers are available,
                // rather than buffering the entire response content. This is essential for streaming scenarios (such as SSE),
                // as it allows the application to begin processing the response stream immediately, chunk by chunk, without waiting for the full response.
                response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    OpenAiErrorContainerDto? container = null;

                    try
                    {
                        container = JsonSerializer.Deserialize<OpenAiErrorContainerDto>(errorContent);
                    }
                    catch (JsonException ex)
                    {
                        // We represent the status code as the enum item name (e.g., "BadRequest") rather than its numeric value (e.g., 400)
                        // because the name is more human-readable and, if needed, can easily be converted back to its numeric value.
                        throw new AiServiceException(
                            message: "Failed to parse OpenAI API error response during streaming.",
                            statusCode: ValueTypeConverter.ToString(response.StatusCode),
                            rawResponse: errorContent,
                            providerDetails: null,
                            innerException: ex);
                    }

                    if (container == null)
                    {
                        throw new AiServiceException(
                            message: "Response could not be deserialized to error container during streaming.",
                            statusCode: ValueTypeConverter.ToString(response.StatusCode),
                            rawResponse: errorContent,
                            providerDetails: null,
                            innerException: null);
                    }

                    throw new AiServiceException(
                        message: "OpenAI API request failed during streaming.",
                        statusCode: ValueTypeConverter.ToString(response.StatusCode),
                        rawResponse: errorContent,
                        providerDetails: container,
                        innerException: null);
                }

                stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                reader = new StreamReader(stream);
            }
            catch (AiServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new AiServiceException(
                    message: "Unexpected error during chat completion streaming.",
                    statusCode: null,
                    rawResponse: null,
                    providerDetails: null,
                    innerException: ex);
            }

            // We use a 'while (true)' loop here to allow fine-grained exception handling and explicit control over loop termination.
            // This design enables us to wrap the ReadLineAsync call in its own try-catch block, so we can handle IO errors or cancellation
            // separately from JSON parsing errors. The loop is exited explicitly with 'break' when the stream ends, or with 'yield break'
            // when cancellation is requested or the stream end marker is encountered. This approach provides maximum flexibility and clarity
            // for error handling and stream processing in an async iterator context.
            while (true)
            {
                string? line = null;
                try
                {
                    // We can safely use the null-forgiving operator here because 'reader' is only assigned after a successful HTTP response and stream acquisition.
                    // If an exception occurs before 'reader' is initialized, the method will exit before reaching this point.
                    // Therefore, 'reader' is guaranteed to be non-null within the streaming loop.
                    line = await reader!.ReadLineAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    throw new AiServiceException(
                        message: "Error while reading from the OpenAI streaming response.",
                        statusCode: null,
                        rawResponse: null,
                        providerDetails: null,
                        innerException: ex);
                }

                // If 'line == null', we use 'break' to exit the loop. This indicates that the end of the stream has been reached naturally (i.e., there is no more data to read).
                // In contrast, 'yield break' is used elsewhere in the loop to signal an intentional, early termination of the async iterator—such as when cancellation is requested
                // or when the "[DONE]" marker is received from the server. Using 'break' here allows any code after the loop (or in the finally block) to execute before the iterator completes,
                // whereas 'yield break' immediately ends the iterator and returns control to the caller.

                if (line == null)
                    break;

                if (cancellationToken.IsCancellationRequested)
                    yield break;

                // Skip empty lines and lines starting with ':'.
                // According to the SSE (Server-Sent Events) specification, lines beginning with ':' are considered comments and should be ignored by the client.
                // While the OpenAI API does not explicitly guarantee the presence of such comment lines, any SSE-compliant server (including OpenAI's) is permitted to send them.
                // Ignoring these lines ensures robust and future-proof SSE stream parsing.
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(':'))
                    continue;

                // Parse SSE format: "data: {json}"
                if (line.StartsWith("data: ", StringComparison.OrdinalIgnoreCase))
                {
                    var jsonData = line.Substring(6); // Remove "data: " prefix

                    // Check for stream end marker
                    // The stream end marker "[DONE]" is specified by OpenAI and is always sent in uppercase according to their API documentation.
                    // While the "data:" prefix is parsed case-insensitively for robustness, the "[DONE]" marker should be compared case-sensitively,
                    // as OpenAI guarantees this exact casing in all streaming responses.
                    if (jsonData.Trim() == "[DONE]")
                        yield break;

                    OpenAiChatCompletionStreamChunkDto? chunk = null;
                    try
                    {
                        chunk = JsonSerializer.Deserialize<OpenAiChatCompletionStreamChunkDto>(jsonData);
                    }
                    catch (JsonException ex)
                    {
                        // If a chunk cannot be parsed, treat the entire stream as broken and throw an exception.
                        throw new AiServiceException(
                            message: "Failed to parse a streaming chunk from the OpenAI API.",
                            statusCode: null,
                            rawResponse: jsonData,
                            providerDetails: null,
                            innerException: ex);
                    }

                    if (chunk != null)
                        yield return chunk;
                }
            }
        }
        finally
        {
            reader?.Dispose();
            stream?.Dispose();
            response?.Dispose();
        }
    }
}
