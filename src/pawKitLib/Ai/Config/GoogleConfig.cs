namespace pawKitLib.Ai.Config;

public record GoogleConfig : IAiProviderConfig
{
    public string ProviderName => "Google";
    public string ApiKey { get; init; }
    public string Endpoint { get; init; }

    public GoogleConfig(string apiKey, string endpoint)
    {
        ApiKey = apiKey;
        Endpoint = endpoint;
    }
}
