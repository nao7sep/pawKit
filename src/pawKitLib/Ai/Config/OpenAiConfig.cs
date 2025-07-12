namespace pawKitLib.Ai.Config;

public record OpenAiConfig : IAiProviderConfig
{
    public string ProviderName => "OpenAI";
    public string ApiKey { get; init; }
    public string Endpoint { get; init; }

    public OpenAiConfig(string apiKey, string endpoint)
    {
        ApiKey = apiKey;
        Endpoint = endpoint;
    }
}
