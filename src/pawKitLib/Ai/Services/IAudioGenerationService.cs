namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface IAudioGenerationService
{
    Task<JsonNode> GenerateAudioAsync(IAiProviderConfig config, JsonNode request, CancellationToken cancellationToken);
    Task<JsonNode> GenerateAudioAsync(IAiProviderConfig config, dynamic request, CancellationToken cancellationToken);
}
