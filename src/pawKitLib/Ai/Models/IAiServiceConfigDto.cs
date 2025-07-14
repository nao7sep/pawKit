namespace pawKitLib.Ai.Models;

public interface IAiServiceConfigDto
{
    // Provider is not get-only for two reasons:
    // 1) In some scenarios, localization (e.g., Japanese) may be required.
    // 2) Allowing setters by default can improve productivity, as it avoids premature restrictions without significant security risk.
    string Provider { get; set; }
    string Endpoint { get; set; }
    string ApiKey { get; set; }
}
