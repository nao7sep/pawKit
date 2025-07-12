namespace pawKitLib.Ai.Requests;

/// <summary>
/// Specifies a conceptual constraint on the format of the model's response.
/// The provider-specific client is responsible for translating this into the appropriate API request.
/// </summary>
public enum ResponseFormat
{
    /// <summary>
    /// The model will return a standard text response (default).
    /// </summary>
    Text,
    /// <summary>
    /// The model is constrained to output a valid JSON object.
    /// </summary>
    JsonObject
}