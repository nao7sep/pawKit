using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using pawKitLib.Ai.Config;
using pawKitLib.Ai.Services;

namespace pawKitLib.Ai.Services.OpenAi
{
    /// <summary>
    /// OpenAI client implementing chat functionality using Flurl.
    /// </summary>
    public class OpenAiClient : IChatService
    {
        private readonly IFlurlClient _flurlClient;

        public OpenAiClient(IFlurlClient flurlClient)
        {
            _flurlClient = flurlClient;
        }

        /// <inheritdoc />
        public async Task<JsonNode> ChatAsync(IAiProviderConfig config, JsonNode request, CancellationToken cancellationToken)
        {
            return await ChatAsyncInternal(config, request.ToJsonString(), cancellationToken);
        }

        /// <summary>
        /// Dynamic overload for ChatAsync.
        /// </summary>
        public async Task<JsonNode> ChatAsync(IAiProviderConfig config, dynamic request, CancellationToken cancellationToken)
        {
            return await ChatAsyncInternal(config, JsonSerializer.Serialize(request), cancellationToken);
        }

        private async Task<JsonNode> ChatAsyncInternal(IAiProviderConfig config, string json, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<JsonNode> ChatStreamAsync(IAiProviderConfig config, JsonNode request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var json = request.ToJsonString();
            await foreach (var item in ChatStreamAsyncInternal(config, json, cancellationToken))
                yield return item;
        }

        /// <summary>
        /// Dynamic overload for ChatStreamAsync.
        /// </summary>
        public async IAsyncEnumerable<JsonNode> ChatStreamAsync(IAiProviderConfig config, dynamic request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            // Explicitly declare json as string to avoid compiler issues with dynamic and async foreach
            string json = JsonSerializer.Serialize(request);
            await foreach (var item in ChatStreamAsyncInternal(config, json, cancellationToken))
                yield return item;
        }

        private async IAsyncEnumerable<JsonNode> ChatStreamAsyncInternal(IAiProviderConfig config, string json, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield break; // Not implemented
        }
    }
}
