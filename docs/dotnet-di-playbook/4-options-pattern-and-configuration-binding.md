# Options Pattern & Configuration Binding

> **Document 4 of 8 – Comprehensive guide to **``** and flexible configuration in .NET**

---

## Table of Contents

1. [Overview](#1-overview)
2. [Configuration Sources & Providers](#2-configuration-sources--providers)
3. [Defining Option Classes](#3-defining-option-classes)
4. [Binding & Registering Options](#4-binding--registering-options)
   1. `Configure<T>()` – basics
   2. Chaining `Bind` + `Configure`
   3. `IConfigureOptions<T>` & `IPostConfigureOptions<T>`
5. [Options Interfaces Deep‑Dive](#5-options-interfaces-deep-dive)
   1. `IOptions<T>` (singleton snapshot)
   2. `IOptionsSnapshot<T>` (scoped snapshot)
   3. `IOptionsMonitor<T>` (live monitor)
6. [Validation Strategies](#6-validation-strategies)
   1. Data‑annotations validation
   2. Custom `Validate` delegates
   3. Implementing `IValidateOptions<T>`
7. [Live Reload & Change Notifications](#7-live-reload--change-notifications)
   1. File change tokens
   2. `OnChange` callbacks
8. [Usage Patterns](#8-usage-patterns)
   1. Injecting options into services
   2. Combining multiple config sources (JSON + env + secrets + vaults)
   3. Overriding individual properties at runtime (API keys)
9. [Testing Options](#9-testing-options)
10. [Anti‑Patterns & Pitfalls](#10-anti-patterns--pitfalls)
11. [Cheatsheet](#11-cheatsheet)
12. [Further Reading](#12-further-reading)

---

## 1. Overview

The **Options Pattern** provides a structured, type‑safe approach for supplying configuration data to services through dependency injection. Instead of sprinkling `Configuration["Key"]` lookups throughout code, we bind settings to **POCO** classes and inject them using ``** family** interfaces.

---

## 2. Configuration Sources & Providers

| Provider                            | Typical Use                                                   |
| ----------------------------------- | ------------------------------------------------------------- |
| `appsettings.json`                  | File‑based defaults, hierarchies via `appsettings.{env}.json` |
| Environment variables               | Container / CI overrides                                      |
| User secrets (Development)          | Local secrets (`dotnet‑user‑secrets`)                         |
| Azure App Configuration / Key Vault | Managed secrets, feature flags                                |
| Command‑line args                   | Immediate overrides                                           |
| In‑memory collection                | Test stubs, dynamic values                                    |

`Host.CreateDefaultBuilder()` wires most of these automatically.

---

## 3. Defining Option Classes

```csharp
public class ApiSettings
{
    public string BaseUrl { get; set; } = "";
    public string ApiKey  { get; set; } = "";
    [Range(1, 10)]
    public int RetryCount { get; set; } = 3;
}
```

*Must be a non‑abstract public class with settable properties.*

---

## 4. Binding & Registering Options

### 4.1 Basic Binding

```csharp
services.Configure<ApiSettings>(config.GetSection("ApiSettings"));
```

### 4.2 Chaining `Bind` + `Configure` (merge JSON + runtime values)

```csharp
var apiKey = Environment.GetEnvironmentVariable("API_KEY");
services.AddOptions<ApiSettings>()
        .Bind(config.GetSection("ApiSettings"))
        .Configure(opts => opts.ApiKey = apiKey);
```

### 4.3 `IConfigureOptions<T>` & `IPostConfigureOptions<T>`

```csharp
public class ApiKeyConfigurator : IConfigureOptions<ApiSettings>
{
    private readonly ISecureStore _store;
    public ApiKeyConfigurator(ISecureStore store) => _store = store;
    public void Configure(ApiSettings opts) => opts.ApiKey = _store.FetchKey();
}
services.AddSingleton<IConfigureOptions<ApiSettings>, ApiKeyConfigurator>();
```

*`IPostConfigureOptions<T>`** runs ****after**** all **`IConfigureOptions<T>`** for final tweaks.*

---

## 5. Options Interfaces Deep‑Dive

| Interface             | Lifetime  | Reload Behavior          | Typical Injection Target |
| --------------------- | --------- | ------------------------ | ------------------------ |
| `IOptions<T>`         | Singleton | Static snapshot          | Singleton services       |
| `IOptionsSnapshot<T>` | Scoped    | New per scope/request    | Controllers, Razor Pages |
| `IOptionsMonitor<T>`  | Singleton | Live update + `OnChange` | Background services      |

### 5.1 Example – Monitor

```csharp
public class Updater(IOptionsMonitor<ApiSettings> monitor)
{
    public Updater()
    {
        monitor.OnChange(newCfg => Console.WriteLine($"BaseUrl now {newCfg.BaseUrl}"));
    }
}
```

---

## 6. Validation Strategies

### 6.1 Data‑Annotations

```csharp
services.AddOptions<ApiSettings>()
        .Bind(config.GetSection("ApiSettings"))
        .ValidateDataAnnotations();
```

### 6.2 Delegate Validation

```csharp
services.AddOptions<ApiSettings>()
        .Bind(config.GetSection("ApiSettings"))
        .Validate(opt => Uri.IsWellFormedUriString(opt.BaseUrl, UriKind.Absolute),
                  "BaseUrl must be absolute");
```

### 6.3 Custom `IValidateOptions<T>`

```csharp
public class ApiSettingsValidator : IValidateOptions<ApiSettings>
{
    public ValidateOptionsResult Validate(string? name, ApiSettings opts) =>
        opts.RetryCount <= 0
            ? ValidateOptionsResult.Fail("RetryCount must be > 0")
            : ValidateOptionsResult.Success;
}
services.AddSingleton<IValidateOptions<ApiSettings>, ApiSettingsValidator>();
```

*Fail‑fast by enabling **`validateOnBuild:true`** when building the provider.*

---

## 7. Live Reload & Change Notifications

- `appsettings.json` reloadOnChange true ⇒ triggers monitor callbacks.
- Use `IOptionsMonitor<T>.OnChange` to react (e.g., refresh cache).
- `IOptionsSnapshot<T>` picks up latest values automatically **per request** in ASP.NET Core.

---

## 8. Usage Patterns

### 8.1 Injecting Options

```csharp
public class ApiClient(HttpClient http, IOptions<ApiSettings> opts)
{
    private readonly ApiSettings _cfg = opts.Value;
}
```

### 8.2 Combining Multiple Sources

1. JSON sets defaults.
2. `Configure` delegate injects secrets.
3. `IConfigureOptions` pulls feature‑flag values.

### 8.3 Overriding Individual Properties (Runtime Secret Example)

Same pattern as §4.2 – use `.Configure` or custom configurator.

---

## 9. Testing Options

```csharp
var settings = new ApiSettings { BaseUrl = "https://test", ApiKey = "x" };
var opts = Options.Create(settings);
var svc  = new ApiClient(fakeHttp, opts);
```

*For monitor tests:* use `OptionsMonitor<ApiSettings>` via `OptionsFactory` helpers or the **OptionsBuilder** test extensions.

---

## 10. Anti‑Patterns & Pitfalls

1. **Injecting **``** everywhere** – defeats purpose of options.
2. **Storing **``** field** – prefer `opts.Value` snapshot (except monitor/snapshot use‑cases).
3. **Scatter‑shot **``** access** – brittle & magic strings.
4. **Failing to validate** – errors surface only at runtime in prod.
5. **Writing to options objects** – treat them as immutable; modify copies instead.

---

## 11. Cheatsheet

```csharp
// Bind + runtime key + validation
services.AddOptions<ApiSettings>()
        .Bind(config.GetSection("ApiSettings"))
        .Configure(o => o.ApiKey = Environment.GetEnv("API_KEY"))
        .Validate(o => !string.IsNullOrWhiteSpace(o.ApiKey), "API Key missing");

// Inject
public class Foo(IOptions<ApiSettings> cfg) { var url = cfg.Value.BaseUrl; }
```

---

## 12. Further Reading

- Docs: *Options pattern in ASP.NET Core* (learn.microsoft.com)
- Andrew Lock – *Options pattern, data annotations & reload*
- Steve Gordon – *Configuring multiple named HttpClients with Options*

---

© 2025 DI Playbook – License: MIT

