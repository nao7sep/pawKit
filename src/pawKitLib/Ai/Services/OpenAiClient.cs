using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using pawKitLib.Ai.Config;
using pawKitLib.Ai.Services;

namespace pawKitLib.Ai.Services
{
    public class OpenAiClient : IChatService
    {
        private readonly IFlurlClient _flurlClient;

        public OpenAiClient(IFlurlClient flurlClient)
        {
            _flurlClient = flurlClient;
        }

        public async Task<string> ChatAsync(IAiProviderConfig config, string request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async IAsyncEnumerable<string> ChatStreamAsync(IAiProviderConfig config, string request, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            yield break;
        }
    }
}
