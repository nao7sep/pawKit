namespace pawKitLib.Ai.Services;

using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Ai.Config;

public interface IChatService
{
    Task<string> ChatAsync(IAiProviderConfig config, string request, CancellationToken cancellationToken);
    IAsyncEnumerable<string> ChatStreamAsync(IAiProviderConfig config, string request, CancellationToken cancellationToken);
}
