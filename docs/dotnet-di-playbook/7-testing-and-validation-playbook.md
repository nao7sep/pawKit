# Testing & Validation Playbook

> **Document 7 of 8 – Strategies for unit, integration, and container‑graph validation in DI‑based .NET applications**

---

## Table of Contents

1. [Overview](#1-overview)
2. [Unit‑Test Friendly Service Wiring](#2-unit-test-friendly-service-wiring)
   1. Minimal `ServiceCollection` setup
   2. Stub & mock patterns
3. [Mocking & Fakes](#3-mocking--fakes)
   1. `IOptions<T>` helpers
   2. `HttpMessageHandler` for HttpClient
   3. Fake `DbContext` / in‑memory database
4. [Integration Testing with Generic Host](#4-integration-testing-with-generic-host)
   1. `HostBuilder` test harness
   2. Overriding services for tests (`ConfigureTestServices`)
5. [ASP.NET Core WebApplicationFactory](#5-aspnet-core-webapplicationfactory)
   1. Creating factory subclasses
   2. Per‑test service overrides
   3. End‑to‑end HTTP assertions
6. [Container Validation Techniques](#6-container-validation-techniques)
   1. `validateOnBuild` & `validateScopes`
   2. Scrutor `AssemblyScan` diagnostics
   3. Graph snapshots for regression testing
7. [Automated Coverage of Lifetimes](#7-automated-coverage-of-lifetimes)
   1. Detecting captive dependencies in CI
   2. Custom xUnit theory for lifetime rules
8. [Example Test Projects](#8-example-test-projects)
   1. Unit test sample (xUnit + Moq)
   2. Worker‑service integration test
   3. Web API functional test with TestServer
9. [Cheatsheet](#9-cheatsheet)
10. [Further Reading](#10-further-reading)

---

## 1. Overview

Solid testing starts with **repeatable service wiring**. This playbook shows how to spin up *just enough* of the DI container for each test level—unit, integration, functional—and how to validate container configuration automatically.

---

## 2. Unit‑Test Friendly Service Wiring

### 2.1 Minimal `ServiceCollection`

```csharp
var services = new ServiceCollection();
services.AddLogging(); // if your service needs ILogger<T>
services.AddSingleton<IMath, MathStub>();
var sp = services.BuildServiceProvider();
```

Use only the **dependencies under test**—avoid full `HostBuilder` unless needed.

### 2.2 Stub & Mock Patterns

- **Moq / NSubstitute** – quick interface mocks.
- **Stub implementations** – lightweight classes replacing real infra (e.g., `FakeClock`).

---

## 3. Mocking & Fakes

### 3.1 `IOptions<T>`

```csharp
var opts = Options.Create(new ApiSettings { BaseUrl = "http://test" });
services.AddSingleton<IOptions<ApiSettings>>(opts);
```

### 3.2 Mocking `HttpClient`

```csharp
var handler = new Mock<HttpMessageHandler>();
handler.Protected()
       .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
       .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
services.AddSingleton(new HttpClient(handler.Object) { BaseAddress = new("http://fake") });
```

### 3.3 In‑Memory `DbContext`

```csharp
services.AddDbContext<AppDb>(o => o.UseInMemoryDatabase("TestDB"));
```

---

## 4. Integration Testing with Generic Host

### 4.1 `HostBuilder` Harness

```csharp
var host = new HostBuilder()
    .ConfigureServices((ctx, s) => s.AddMyApplication())
    .Build();
```

### 4.2 Override Services for Tests

```csharp
host.Services.GetRequiredService<IServiceCollection>()
    .AddSingleton<IPayment, FakePayment>();
```

---

## 5. ASP.NET Core WebApplicationFactory

### 5.1 Subclass Example

```csharp
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(s =>
        {
            s.RemoveAll<IPayment>();
            s.AddSingleton<IPayment, FakePayment>();
        });
    }
}
```

### 5.2 Usage in Tests

```csharp
var client = _factory.CreateClient();
var res = await client.GetAsync("/health");
res.StatusCode.Should().Be(HttpStatusCode.OK);
```

---

## 6. Container Validation Techniques

### 6.1 Build‑Time Flags

Enable in unit test startup:

```csharp
services.BuildServiceProvider(validateOnBuild: true, validateScopes: true);
```

### 6.2 Scrutor Diagnostics

```csharp
services.Scan(s => s.FromCallingAssembly().AddClasses().AsImplementedInterfaces());
// Scrutor throws on duplicates by default if not UsingTryAdd
```

### 6.3 Graph Snapshots

Serialize `ServiceCollection` descriptors to JSON and compare in regression tests to catch **breaking registration changes**.

---

## 7. Automated Coverage of Lifetimes

### 7.1 Captive Dependency Detection in CI

Custom analyzer sample (pseudo‑code):

```csharp
foreach (var d in services)
    if (d.Lifetime == Singleton && d.DependsOn<Lifetime.Scoped>())
        Assert.Fail($"Captive dep: {d.ServiceType}");
```

### 7.2 Lifetime Theory Example (xUnit)

```csharp
[Theory]
[MemberData(nameof(Descriptors))]
public void Transients_should_not_be_heavy(ServiceDescriptor d)
{
    if (d.Lifetime == ServiceLifetime.Transient)
        Assert.True(d.ImplementationType.GetCustomAttribute<LightweightAttribute>() != null);
}
```

---

## 8. Example Test Projects

| Level          | Frameworks                         | Purpose                                |
| -------------- | ---------------------------------- | -------------------------------------- |
| Unit           | xUnit + Moq                        | Business logic in isolation            |
| Integration    | HostBuilder + In‑Memory DB         | Service layer + EF Core interactions   |
| Functional API | WebApplicationFactory + TestServer | End‑to‑end HTTP + middleware + routing |

---

## 9. Cheatsheet

```csharp
// Minimal container for unit test
var sp = new ServiceCollection()
            .AddSingleton<IClock, FakeClock>()
            .BuildServiceProvider();

// Validate lifetimes in CI
services.BuildServiceProvider(true, true);

// Mock IOptions
var opts = Options.Create(new MyCfg { Flag = true });
services.AddSingleton<IOptions<MyCfg>>(opts);
```

---

## 10. Further Reading

- Docs: *Integration tests in ASP.NET Core* (learn.microsoft.com)
- Andrew Lock – *Testing with WebApplicationFactory*
- EF Core Docs – *Testing with InMemory provider*
- Steve Gordon – *HttpClient unit testing patterns*

---

© 2025 DI Playbook – License: MIT

