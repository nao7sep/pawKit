namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Provides configuration options for the <see cref="OpenAiClient"/>.
/// </summary>
public sealed record OpenAiOptions
{
    /// <summary>The API key for authenticating with the OpenAI API.</summary>
    public required string ApiKey { get; init; }

    /// <summary>The base URL for the OpenAI API. Defaults to the official v1 endpoint.</summary>
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";
}