namespace pawKitLib.Ai.Providers.OpenAI;

/// <summary>
/// Defines constant string values used for interacting with the OpenAI API.
/// </summary>
internal static class OpenAiApiConstants
{
    // Endpoints
    public const string ChatCompletionsEndpoint = "chat/completions";

    // Auth
    public const string BearerAuthenticationScheme = "Bearer";

    // Roles
    public const string RoleSystem = "system";
    public const string RoleUser = "user";
    public const string RoleAssistant = "assistant";
    public const string RoleTool = "tool";

    // Tool Choice
    public const string ToolChoiceNone = "none";
    public const string ToolChoiceAuto = "auto";
    public const string ToolChoiceRequired = "required";

    // Response Format
    public const string ResponseFormatJsonObject = "json_object";
}