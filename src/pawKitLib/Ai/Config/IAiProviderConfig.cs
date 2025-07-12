namespace pawKitLib.Ai.Config;

public interface IAiProviderConfig
{
    string ProviderName { get; }
    string ApiKey { get; }
    string Endpoint { get; }
}
