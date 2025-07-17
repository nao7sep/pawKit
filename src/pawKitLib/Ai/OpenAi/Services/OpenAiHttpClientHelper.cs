using System.Text.Json;
using pawKitLib.Ai.OpenAi.Models;
using pawKitLib.Conversion;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace pawKitLib.Ai.OpenAi.Services;

public static class OpenAiHttpClientHelper
{
    // All exceptions are classified and wrapped as AiServiceException to provide a unified error contract for consumers of the AI namespace.
    // This allows callers to easily distinguish AI-related errors from other types (e.g., IO, database) and handle them appropriately.
    // It also ensures that all relevant context (status code, raw response, provider details, inner exception) is consistently available.

    /// <summary>
    /// Sends an HTTP request to the OpenAI API and handles error classification and deserialization.
    /// Accepts any HttpRequestMessage, allowing for flexible content and headers.
    /// All errors are wrapped as AiServiceException for consistent handling.
    /// </summary>
    public static async Task<TResponse> SendAsync<TResponse, TService>(
        ILogger<TService> logger,
        HttpClient client,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // May throw HttpRequestException, TaskCanceledException, or ObjectDisposedException
            var response = await client.SendAsync(request, cancellationToken);

            // May throw ObjectDisposedException, IOException
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("Received JSON response from OpenAI API: {@Json}", json);

            if (response.IsSuccessStatusCode)
            {
                // May throw JsonException, ArgumentNullException
                var result = JsonSerializer.Deserialize<TResponse>(json);
                if (result == null)
                {
                    throw new AiServiceException(
                        message: $"Response could not be deserialized to {typeof(TResponse).Name}.",
                        statusCode: ValueTypeConverter.ToString(response.StatusCode),
                        rawResponse: json,
                        providerDetails: null,
                        innerException: null
                    );
                }
                return result;
            }

#if DEBUG
            // If the response is not successful, we immediately output the raw JSON to the debugger.
            // This ensures the error details are visible during debugging as early as possible.
            Debug.WriteLine(json);
#endif

            // Any non-success HTTP response is always treated as an error, regardless of content.
            // This enforces strict API contract handling and avoids silent failures.

            // May throw JsonException, ArgumentNullException
            var container = JsonSerializer.Deserialize<OpenAiErrorContainerDto>(json);

            // We do not inspect or branch on container.Error here, even though it is the most common use case.
            // This avoids coupling to current API details and keeps the method open for future changes.
            if (container == null)
            {
                throw new AiServiceException(
                    message: "Response could not be deserialized to error container.",
                    statusCode: ValueTypeConverter.ToString(response.StatusCode),
                    rawResponse: json,
                    providerDetails: null,
                    innerException: null
                );
            }

            // We attach the entire container as providerDetails, not just the error property, to allow for future extensibility.
            // While currently only the error property is used, the container may include additional information in future API versions.
            // This design keeps the library open for extension without requiring breaking changes.
            throw new AiServiceException(
                message: "Request failed.",
                statusCode: ValueTypeConverter.ToString(response.StatusCode),
                rawResponse: json,
                providerDetails: container,
                innerException: null
            );
        }
        // Rethrow if already an AiServiceException
        catch (AiServiceException)
        {
            throw;
        }
        // Network-related errors
        catch (HttpRequestException ex)
        {
            throw new AiServiceException(
                message: "Network error during OpenAI API call.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
        // JSON deserialization errors
        catch (JsonException ex)
        {
            throw new AiServiceException(
                message: "Failed to parse OpenAI API response.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
        // Any other unexpected errors
        catch (Exception ex)
        {
            throw new AiServiceException(
                message: "Unexpected error during OpenAI API call.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }

    /// <summary>
    /// Sends an HTTP request to the OpenAI API that returns a binary response (e.g., audio file).
    /// Handles error classification but does not attempt to deserialize the response.
    /// Returns the raw byte array of the response content.
    /// </summary>
    public static async Task<byte[]> SendForBinaryResponseAsync<TService>(
        ILogger<TService> logger,
        HttpClient client,
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // In this method, we must check IsSuccessStatusCode before reading the response content.
            // A successful response returns a binary payload (such as audio), while an error response returns JSON.
            // Attempting to read the content as bytes when the response is actually an error (JSON) would result in incorrect handling.
            // By contrast, SendAsync can always read the content as JSON, since both success and error responses are expected to be JSON.
            var response = await client.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                // May throw HttpRequestException, InvalidOperationException
                var contentBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                return contentBytes;
            }

            // Handle errors as before
            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogDebug("Received JSON response from OpenAI API: {@Json}", json);

#if DEBUG
            // If the response is not successful, we immediately output the raw JSON to the debugger.
            // This ensures the error details are visible during debugging as early as possible.
            Debug.WriteLine(json);
#endif

            var container = JsonSerializer.Deserialize<OpenAiErrorContainerDto>(json);
            if (container == null)
            {
                throw new AiServiceException(
                    message: "Response could not be deserialized to error container.",
                    statusCode: ValueTypeConverter.ToString(response.StatusCode),
                    rawResponse: json,
                    providerDetails: null,
                    innerException: null
                );
            }

            throw new AiServiceException(
                message: "Request failed.",
                statusCode: ValueTypeConverter.ToString(response.StatusCode),
                rawResponse: json,
                providerDetails: container,
                innerException: null
            );
        }
        catch (AiServiceException)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new AiServiceException(
                message: "Network error during OpenAI API call.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
        catch (JsonException ex)
        {
            throw new AiServiceException(
                message: "Failed to parse OpenAI API response.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
        catch (Exception ex)
        {
            throw new AiServiceException(
                message: "Unexpected error during OpenAI API call.",
                statusCode: null,
                rawResponse: null,
                providerDetails: null,
                innerException: ex);
        }
    }
}
