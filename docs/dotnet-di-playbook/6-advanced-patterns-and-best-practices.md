# Advanced Patterns & Best Practices

> **Document 6 of 8 – Level‑up your DI architecture with proven patterns and cross‑cutting techniques**

---

## Table of Contents

1. [Overview](#1-overview)
2. [Taming Constructor Bloat](#2-taming-constructor-bloat)
   1. Facade / Orchestrator services
   2. Aggregates & vertical‑slice handlers
3. [Decorator Pattern](#3-decorator-pattern)
   1. Manual wiring
   2. Scrutor helpers
   3. Ordering & multiple decorators
4. [Mediator, CQRS & Pipeline Behaviors](#4-mediator-cqrs--pipeline-behaviors)
   1. MediatR quick‑start
   2. Command‑query separation with DI
   3. Custom pipeline behaviors (logging, validation, metrics)
5. [Open‑Generic Registrations & Constraints](#5-open-generic-registrations--constraints)
   1. Standard `typeof(IRepo<>)` pattern
   2. Adding constraints (where T : class)
   3. Generic decorators & Scrutor `.AddClasses()`
6. [Cross‑Cutting Concerns](#6-cross-cutting-concerns)
   1. Logging scopes & correlation IDs
   2. Distributed tracing (ActivitySource)
   3. Metrics (Prometheus, OpenTelemetry exporters)
7. [Aspect‑Oriented Techniques](#7-aspect-oriented-techniques)
   1. Interception with Autofac / Castle DynamicProxy
   2. Source generators & compile‑time weaving (Fody, AspectInjector)
8. [Modular Service Registration](#8-modular-service-registration)
   1. Feature modules (`IServiceCollection` extensions)
   2. Conventions & assembly scanning
9. [Packaging Reusable Libraries](#9-packaging-reusable-libraries)
   1. Exposing `IServiceCollection` extension methods
   2. Using `TryAdd*` for safe defaults
   3. Avoiding versioning collisions (generic host builder pattern)
10. [Performance & Memory Tips](#10-performance--memory-tips)
11. [Cheatsheet](#11-cheatsheet)
12. [Further Reading](#12-further-reading)

---

## 1. Overview

After mastering lifetimes and registration basics, complex projects need **architectural patterns** that keep code maintainable as feature‑sets grow. This document collects the most common high‑level techniques used in production .NET systems and shows how they integrate with Microsoft.Extensions.DependencyInjection (MS.DI).

---

## 2. Taming Constructor Bloat

> **Problem:** Services or controllers receive 8–10 dependencies, making them hard to test and change.

### 2.1 Facade / Orchestrator Services

Combine multiple low‑level services behind one higher‑level abstraction.

```csharp
public class OrderOrchestrator(IInventory inv, IPayment pay, INotifier note)
{
    public async Task PlaceOrderAsync(OrderDto dto)
    {
        await inv.ReserveAsync(dto);
        await pay.ChargeAsync(dto);
        await note.EmailAsync(dto);
    }
}
```

Controller now injects **one** dependency:

```csharp
public OrdersController(OrderOrchestrator orchestrator) { ... }
```

### 2.2 Vertical‑Slice Handlers

*Inspired by Jimmy Bogard’s vertical‑slice architecture.*  One handler per feature:

```csharp
public record CreateUserCmd(string Email) : IRequest<Guid>;
public class CreateUserHandler(IUserRepo repo, IEmail email) : IRequestHandler<CreateUserCmd, Guid>
{
    public async Task<Guid> Handle(CreateUserCmd cmd, CancellationToken ct)
    {
        var id = await repo.AddAsync(cmd.Email, ct);
        await email.WelcomeAsync(cmd.Email);
        return id;
    }
}
```

---

## 3. Decorator Pattern

Wrap a core service with additional behavior (caching, logging, metrics).

### 3.1 Manual Wiring

```csharp
services.AddSingleton<IWeatherService, WeatherService>();
services.AddSingleton<IWeatherService>(sp =>
    new CachedWeatherService(sp.GetRequiredService<WeatherService>(),
                              sp.GetRequiredService<IMemoryCache>()));
```

### 3.2 Scrutor Helpers

```csharp
services.Decorate<IWeatherService, CachedWeatherService>();
services.Decorate<IWeatherService, MetricsWeatherService>();
```

Scrutor maintains registration order; first registered decorator is **outermost**.

### 3.3 Ordering Tips

- Outer decorators = cross‑cutting (metrics, logging)
- Inner decorators = feature‑specific (caching)

---

## 4. Mediator, CQRS & Pipeline Behaviors

### 4.1 MediatR Quick‑Start

```csharp
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateUserHandler>());
```

`IRequest<TResp>` – command/query messages resolved automatically via DI.

### 4.2 CQRS Separation

- **Commands** mutate state – return `Unit` or ID.
- **Queries** read state – use read‑optimized DTOs.

### 4.3 Pipeline Behaviors

```csharp
public class LoggingBehavior<TReq,TRes>(ILogger<LoggingBehavior<TReq,TRes>> log)
    : IPipelineBehavior<TReq,TRes>
{
    public async Task<TRes> Handle(TReq request, RequestHandlerDelegate<TRes> next, CancellationToken ct)
    {
        using var scope = log.BeginScope("Handling {Type}", typeof(TReq).Name);
        var res = await next();
        return res;
    }
}
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

Behaviors are decorators around every MediatR handler, perfect for validation, authorization, metrics.

---

## 5. Open‑Generic Registrations & Constraints

### 5.1 Standard Pattern

```csharp
services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
```

### 5.2 Adding Constraints (Autofac example)

If you need `where T : IEntity` constraints, MS.DI can’t enforce; use Autofac’s `AsClosedTypesOf` for finer control.

### 5.3 Generic Decorators

Scrutor supports open‑generic decorate:

```csharp
services.Decorate(typeof(IRepository<>), typeof(CachingRepoDecorator<>));
```

---

## 6. Cross‑Cutting Concerns

### 6.1 Logging Scopes & Correlation IDs

Inject `ILogger<T>` and add a scope per request/message:

```csharp
using var scope = _log.BeginScope(new { TraceId = traceId });
```

Propagate `TraceId` via middleware, gRPC interceptors, or message metadata.

### 6.2 Distributed Tracing

Use `System.Diagnostics.ActivitySource` + OpenTelemetry exporter:

```csharp
services.AddOpenTelemetry().WithTracing(t => t
    .AddAspNetCoreInstrumentation()
    .AddSource("MyApp.*")
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyApp")));
```

### 6.3 Metrics

Expose Prometheus counters in decorators or pipeline behaviors.

---

## 7. Aspect‑Oriented Techniques

*MS.DI has no interception.*  Choose:

1. **Autofac + Castle DynamicProxy** – `EnableInterfaceInterceptors()`.
2. **Compile‑time weaving** – Fody’s `MethodDecorator` or Explore Source Generators.

*Use sparingly; debugging is harder.*

---

## 8. Modular Service Registration

Large solutions benefit from feature modules:

```csharp
public static class InventoryModule
{
    public static IServiceCollection AddInventory(this IServiceCollection s)
    {
        return s.AddScoped<IInventoryService, InventoryService>()
                .AddScoped<IStockRepo, StockRepo>();
    }
}
```

Main `Program.cs` stays clean:

```csharp
builder.Services.AddInventory()
                .AddPayments()
                .AddShipping();
```

Assembly scanning with Scrutor:

```csharp
services.Scan(s => s.FromAssemblies(assemblies)
    .AddClasses(c => c.Where(t => t.Name.EndsWith("Service")))
    .AsImplementedInterfaces()
    .WithScopedLifetime());
```

---

## 9. Packaging Reusable Libraries

- Provide an `` extension.
- Use `TryAdd*` to avoid overriding consumer registrations.
- Expose options via `services.Configure<MyLibOptions>()` for flexibility.

---

## 10. Performance & Memory Tips

| Tip                                   | Why it helps               |
| ------------------------------------- | -------------------------- |
| Prefer singleton stateless services   | Fewer allocations          |
| Pool heavy objects (e.g., regex, buf) | Reduce GC pressure         |
| Validate container graph in CI        | Catch captive deps early   |
| Measure using `dotnet-counters`       | Spot resolve‑time hotspots |

---

## 11. Cheatsheet

```csharp
// Decorate open generic repository
services.AddScoped(typeof(IRepo<>), typeof(EfRepo<>));
services.Decorate(typeof(IRepo<>), typeof(CacheRepoDec<>));

// Add MediatR + pipeline behaviors
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Marker>());
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));

// Module registration
builder.Services.AddInventory().AddPayments();
```

---

## 12. Further Reading

- Jimmy Bogard – *Vertical Slice Architecture*
- Steve Gordon – *Decorators with Scrutor*
- Microsoft Docs – *OpenTelemetry instrumentation in .NET*
- Mark Seemann – *Dependency Injection antipatterns revisited*

---

© 2025 DI Playbook – License: MIT

