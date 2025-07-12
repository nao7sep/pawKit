namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface IImageGenerationService
{
    Task<JsonNode> GenerateImageAsync(IAiProviderConfig config, JsonNode request, CancellationToken cancellationToken);
    Task<JsonNode> GenerateImageAsync(IAiProviderConfig config, dynamic request, CancellationToken cancellationToken);
}
