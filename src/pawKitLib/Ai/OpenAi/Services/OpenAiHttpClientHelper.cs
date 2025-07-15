using System;
using System.Text.Json;
using pawKitLib.Ai.OpenAi.Models;
using pawKitLib.Conversion;

namespace pawKitLib.Ai.OpenAi.Services;

public static class OpenAiHttpClientHelper
{
    // All exceptions are classified and wrapped as AiServiceException to provide a unified error contract for consumers of the AI namespace.
    // This allows callers to easily distinguish AI-related errors from other types (e.g., IO, database) and handle them appropriately.
    // It also ensures that all relevant context (status code, raw response, provider details, inner exception) is consistently available.

    public static async Task<TResponse> PostMultipartAsync<TResponse>(
        HttpClient client,
        string endpoint,
        MultipartFormDataContent content,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // May throw HttpRequestException, TaskCanceledException, or ObjectDisposedException
            var response = await client.PostAsync(endpoint, content, cancellationToken);

            // May throw ObjectDisposedException, IOException
            var json = await response.Content.ReadAsStringAsync(cancellationToken);

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
}
