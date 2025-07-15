# HttpClient, IHttpClientFactory & External‑API Integration

> **Document 5 of 8 – Design reliable, high‑performance outbound HTTP layers in .NET**

---

## Table of Contents

1. [Overview](#1-overview)
2. [Why Not ](#2-why-not-new-httpclient-per-call)[`new HttpClient()`](#2-why-not-new-httpclient-per-call)[ per Call?](#2-why-not-new-httpclient-per-call)
   1. Socket exhaustion
   2. DNS‑pinning & stale connections
3. [IHttpClientFactory Fundamentals](#3-ihttpclientfactory-fundamentals)
   1. Lifecycle & pooling model
   2. Default handler lifetime (2 min)
   3. Logging integration
4. [Registration Patterns](#4-registration-patterns)
   1. Typed clients
   2. Named clients
   3. Generic `AddHttpClient`
5. [Typed‑Client Design](#5-typed-client-design)
   1. Creating a typed client class
   2. Injecting dependencies (ILogger, IOptions)
   3. Strong typing vs raw `HttpClient`
6. [Advanced Configuration](#6-advanced-configuration)
   1. Per‑client default headers, base address
   2. Delegating handlers & middleware
   3. Polly resilience policies (retry, circuit‑breaker, timeout)
7. [Authentication & Authorization](#7-authentication--authorization)
   1. Bearer tokens via `ITokenAcquisition` (MSAL)
   2. App‑token renewal handlers
8. [Performance & Diagnostics](#8-performance--diagnostics)
   1. Sockets telemetry (`dotnet-counters`)
   2. Logging scopes & correlation IDs
   3. HTTP/2 & HTTP/3 considerations
9. [Testing Outbound HTTP](#9-testing-outbound-http)
   1. Mocking `HttpMessageHandler`
   2. WireMock.Net / MockHttp approaches
   3. Integration testing with TestServer
10. [Anti‑Patterns & Pitfalls](#10-anti-patterns--pitfalls)
11. [Cheatsheet](#11-cheatsheet)
12. [Further Reading](#12-further-reading)

---

## 1. Overview

Outbound HTTP calls are **frequent failure points**. The .NET `` class is powerful but easy to misuse. `IHttpClientFactory` (introduced in ASP.NET Core 2.1) solves common issues by centralizing client creation, handler pooling, and configuration.

---

## 2. Why Not `new HttpClient()` per Call?

| Issue           | Cause                                                          | Impact                                    |
| --------------- | -------------------------------------------------------------- | ----------------------------------------- |
| **Socket leak** | Each `HttpClient` creates a `SocketsHttpHandler` → new sockets | **Socket exhaustion** after \~2 K clients |
| **DNS pinning** | Handler caches DNS forever by default                          | Calls hit stale IP after DNS change       |
| **Performance** | TLS handshake per client                                       | Slower, more CPU                          |

> Bottom line: **reuse handlers** instead of creating new ones.

---

## 3. IHttpClientFactory Fundamentals

- **Singleton** service registered with `AddHttpClient()`.
- Provides **transient **``** instances** backed by a **pooled handler (SocketsHttpHandler)**.
- Default **handler lifetime = 2 minutes** (configurable).
- Automatically adds **Microsoft.Extensions.Logging** integration – every request is logged with category `System.Net.Http.HttpClient.<Name>.LogicalHandler`.

---

## 4. Registration Patterns

### 4.1 Typed Client

```csharp
services.AddHttpClient<WeatherApiClient>(client =>
{
    client.BaseAddress = new("https://api.weather.example/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("WeatherApp/1.0");
});
```

### 4.2 Named Client

```csharp
services.AddHttpClient("GitHub", c => c.BaseAddress = new("https://api.github.com"));
```

Inject via `IHttpClientFactory`:

```csharp
var client = factory.CreateClient("GitHub");
```

### 4.3 Generic Configuration

```csharp
services.AddHttpClient(); // bare minimum, still benefits from pooling
```

---

## 5. Typed‑Client Design

```csharp
public class WeatherApiClient(HttpClient http, IOptions<ApiSettings> cfg, ILogger<WeatherApiClient> log)
{
    public async Task<WeatherDto?> GetCurrentAsync(string city)
    {
        var res = await http.GetAsync($"weather/current?city={city}");
        res.EnsureSuccessStatusCode();
        return await res.Content.ReadFromJsonAsync<WeatherDto>();
    }
}
```

*Advantages*: Strongly‑typed entry point, encapsulated error handling, easy mocking.

---

## 6. Advanced Configuration

### 6.1 Per‑Client Defaults

```csharp
services.AddHttpClient<StockClient>()
        .ConfigureHttpClient(c => c.BaseAddress = new("https://stocks.example"))
        .ConfigurePrimaryHttpMessageHandler(() =>
            new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip });
```

### 6.2 Delegating Handlers

```csharp
services.AddTransient<LoggingHandler>();
services.AddHttpClient<OrderClient>()
        .AddHttpMessageHandler<LoggingHandler>();
```

### 6.3 Polly Policies

```csharp
services.AddHttpClient<PaymentClient>()
        .AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(i)))
        .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(5));
```

---

## 7. Authentication & Authorization

- **Bearer tokens**: inject `ITokenAcquisition` (Microsoft.Identity.Web) in a delegating handler.
- **API keys**: use `IConfigureNamedOptions<HttpClientFactoryOptions>` to add default headers.
- Rotate keys/tokens via `IOptionsMonitor<ApiSettings>` into custom handler.

---

## 8. Performance & Diagnostics

| Tool                | What it shows                                                                 |
| ------------------- | ----------------------------------------------------------------------------- |
| `dotnet-counters`   | `System.Net.Http` counters: requests queued/started/completed, sockets in use |
| ASP.NET Core logs   | `HttpClient` categories with duration, status                                 |
| Wireshark/Fiddler   | Protocol details (HTTP/2 multiplexing)                                        |
| `Diagnose with WCF` | Live HTTP traces (Visual Studio)                                              |

**Tip**: Enable **HTTP/2** by default (`DefaultRequestVersion = HttpVersion.Version20`) when the server supports it.

---

## 9. Testing Outbound HTTP

### 9.1 Mocking `HttpMessageHandler`

```csharp
var handler = new Mock<HttpMessageHandler>();
handler.Protected()
       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
       .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") });
var client = new HttpClient(handler.Object) { BaseAddress = new("http://test") };
```

### 9.2 WireMock.Net / RichardSzalay.MockHttp

Provide fluent in‑memory HTTP servers.

### 9.3 Integration Testing

Use `` or **TestServer** to hit real endpoints in memory.

---

## 10. Anti‑Patterns & Pitfalls

1. ``** per request** – socket leak.
2. **Long‑lived **``** with cookies disabled** – auth fails intermittently.
3. **Swallowing non‑success status codes** – use `EnsureSuccessStatusCode` or domain‑specific handling.
4. **Large request bodies without **`` – causes buffering.
5. **Synchronous **`` – deadlocks under `ConfigureAwait(false)`.

---

## 11. Cheatsheet

```csharp
// Register typed client with retry + timeout
services.AddHttpClient<InvoiceClient>(cfg =>
{
    cfg.BaseAddress = new("https://invoice.example");
    cfg.DefaultRequestVersion = HttpVersion.Version20;
})
.AddTransientHttpErrorPolicy(p => p.WaitAndRetryAsync(3, i => TimeSpan.FromMilliseconds(200 * i)))
.AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(10));

// Use in service
public class BillingService(InvoiceClient inv) {
    public Task SendAsync(Invoice i) => inv.SendAsync(i);
}
```

---

## 12. Further Reading

- Docs: *Make HTTP requests using IHttpClientFactory* (learn.microsoft.com)
- Steve Gordon – *.NET resilience with Polly*
- Andrew Lock – *Using IHttpClientFactory in ASP.NET Core*

---

© 2025 DI Playbook – License: MIT

