namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface ITranslationService
{
    Task<JsonNode> TranslateAsync(IAiProviderConfig config, JsonNode request, CancellationToken cancellationToken);
}
