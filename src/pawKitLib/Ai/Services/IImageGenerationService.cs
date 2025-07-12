namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface IImageGenerationService
{
    Task<string> GenerateImageAsync(IAiProviderConfig config, string request, CancellationToken cancellationToken);
}
