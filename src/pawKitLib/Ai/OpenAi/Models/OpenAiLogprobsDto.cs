using System.Collections.Generic;
using System.Text.Json.Serialization;
using pawKitLib.Models;

namespace pawKitLib.Ai.OpenAi.Models;

/// <summary>
/// Log probability information for tokens.
/// </summary>
public class OpenAiLogprobsDto : DynamicDto
{
    [JsonPropertyName("content")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<OpenAiTokenLogprobDto>? Content { get; set; }
}