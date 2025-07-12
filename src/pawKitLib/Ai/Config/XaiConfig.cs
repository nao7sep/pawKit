namespace pawKitLib.Ai.Config;

public record XaiConfig : IAiProviderConfig
{
    public string ProviderName => "xAI";
    public string ApiKey { get; init; }
    public string Endpoint { get; init; }

    public XaiConfig(string apiKey, string endpoint)
    {
        ApiKey = apiKey;
        Endpoint = endpoint;
    }
}
