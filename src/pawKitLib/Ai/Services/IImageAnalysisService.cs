namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface IImageAnalysisService
{
    Task<JsonNode> AnalyzeImageAsync(IAiProviderConfig config, JsonNode request, CancellationToken cancellationToken);
}
