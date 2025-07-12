namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface IChatService
{
    Task<JsonNode> ChatAsync(IAiProviderConfig config, JsonNode request, CancellationToken cancellationToken);
    IAsyncEnumerable<JsonNode> ChatStreamAsync(IAiProviderConfig config, JsonNode request, CancellationToken cancellationToken);
}
