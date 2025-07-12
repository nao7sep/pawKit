namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface IWebSearchService
{
    Task<JsonNode> SearchAsync(IAiProviderConfig config, JsonNode request, CancellationToken cancellationToken);
}
