namespace pawKitLib.Ai.Config;

public record AnthropicConfig : IAiProviderConfig
{
    public string ProviderName => "Anthropic";
    public string ApiKey { get; init; }
    public string Endpoint { get; init; }

    public AnthropicConfig(string apiKey, string endpoint)
    {
        ApiKey = apiKey;
        Endpoint = endpoint;
    }
}
