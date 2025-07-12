namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface ITranscriptionService
{
    Task<string> TranscribeAsync(IAiProviderConfig config, string request, CancellationToken cancellationToken);
}
