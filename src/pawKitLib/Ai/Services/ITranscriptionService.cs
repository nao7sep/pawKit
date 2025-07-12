namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface ITranscriptionService
{
    Task<JsonNode> TranscribeAsync(IAiProviderConfig config, JsonNode request, CancellationToken cancellationToken);
    Task<JsonNode> TranscribeAsync(IAiProviderConfig config, dynamic request, CancellationToken cancellationToken);
}
