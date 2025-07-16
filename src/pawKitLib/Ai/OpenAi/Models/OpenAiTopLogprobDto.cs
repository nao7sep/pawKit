using System.Collections.Generic;
using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Top log probability information for alternative tokens.
/// </summary>
public class OpenAiTopLogprobDto : DynamicDto
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("logprob")]
    public double Logprob { get; set; }

    [JsonPropertyName("bytes")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<int>? Bytes { get; set; }
}