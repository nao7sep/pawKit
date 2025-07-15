# Dependency Injection Fundamentals in .NET

> **Document 1 of 8 – Core primer for the rest of the playbook**

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Inversion of Control vs. Dependency Injection](#2-inversion-of-control-vs-dependency-injection)
3. [Why Use DI?](#3-why-use-di)
4. [DI in the .NET Ecosystem](#4-di-in-the-net-ecosystem)
5. [The Built‑in DI Container](#5-the-built-in-di-container)
   1. IServiceCollection & ServiceDescriptor
   2. IServiceProvider & Resolve Process
   3. Lifetimes (overview)
6. [Registering Services](#6-registering-services)
   1. Generic overloads
   2. Concrete‑only registration
   3. Factory delegates
7. [Building & Validating the Container](#7-building--validating-the-container)
8. [Injection Styles](#8-injection-styles)
   1. Constructor (recommended)
   2. Method & property injection
9. [Composition Root & Service Locator](#9-composition-root--service-locator)
10. [DI Across Application Types](#10-di-across-application-types)
11. [Comparison with Third‑Party Containers](#11-comparison-with-third-party-containers)
12. [Common Anti‑Patterns & Smells](#12-common-anti-patterns--smells)
13. [Quick‑Start Cheatsheet](#13-quick-start-cheatsheet)
14. [Further Reading](#14-further-reading)

---

## 1. Introduction

Dependency Injection (DI) is the backbone of modern .NET application architecture. This document gives you a **first‑principles understanding** of how and why to use DI, setting the stage for deeper topics covered in later manuals.

## 2. Inversion of Control vs. Dependency Injection

| Concept | One‑liner                                                                                                            | Relationship           |
| ------- | -------------------------------------------------------------------------------------------------------------------- | ---------------------- |
| **IoC** | Code de‑coupling pattern where flow & object creation are moved to a container/framework.                            | Umbrella principle.    |
| **DI**  | Concrete technique of *supplying* a class’s collaborators from the outside instead of instantiating them internally. | Implementation of IoC. |

### 2.1 Historical Context

- *Hollywood principle* – “Don’t call us; we’ll call you.”
- Emerged from Java Spring; embraced in .NET since **ASP.NET Core 1.0 (2016)**.

## 3. Why Use DI?

- **Loose coupling** → easier refactoring & replacement.
- **Testability** → mock dependencies in unit tests.
- **Configurability** → central container controls lifetimes, logging, features.
- **Single Responsibility Principle** → classes focus on behavior, not wiring.

## 4. DI in the .NET Ecosystem

- **Microsoft.Extensions.DependencyInjection (MS.DI)** – lightweight, built‑in, default for ASP.NET/Worker/Minimal API.
- Alternative containers: **Autofac, SimpleInjector, Lamar, Ninject** – add advanced features (keyed services, interception, diagnostics).
- The DI abstraction layer (`IServiceProvider`) lets you swap containers with minimal code changes.

## 5. The Built‑in DI Container

### 5.1 IServiceCollection & ServiceDescriptor

```csharp
var services = new ServiceCollection();
services.AddSingleton<IMail, SmtpMail>();
services.AddTransient<WeatherClient>();
```

*Each **`.Add*`** call creates a **`ServiceDescriptor`** object (service type, implementation type/factory, lifetime).*

### 5.2 IServiceProvider & Resolve Process

```csharp
var provider = services.BuildServiceProvider();
var mail = provider.GetRequiredService<IMail>();
```

1. Provider walks the descriptor graph.
2. Constructs objects **bottom‑up** (recursive resolution).
3. Caches singleton/scoped instances as required.

### 5.3 Lifetimes (overview)

| Lifetime                              | Scope | Created              | Disposed                        |
| ------------------------------------- | ----- | -------------------- | ------------------------------- |
| Singleton                             | App   | 1st request          | App shutdown                    |
| Scoped                                | Scope | 1st request in scope | Scope dispose                   |
| Transient                             | N/A   | Every resolve        | Immediately (if container owns) |
| *Full details in ****Document 2****.* |       |                      |                                 |

## 6. Registering Services

### 6.1 Generic Overloads

```csharp
services.AddSingleton<IMsgBus, RabbitMsgBus>();
services.AddScoped<IRepo<Order>, EfOrderRepo>();
services.AddTransient<Formatter>();
```

### 6.2 Concrete‑only Registration

Good for helper classes with no interface:

```csharp
services.AddSingleton<SlugGenerator>();
```

### 6.3 Factory Delegates

```csharp
services.AddSingleton<IClock>(sp =>
    new UtcClock(offset: TimeSpan.Parse(sp.GetRequiredService<IConfig>().UtcOffset)));
```

## 7. Building & Validating the Container

```csharp
var provider = services.BuildServiceProvider(
    validateScopes: true,    // catch scoped‑in‑singleton errors
    validateOnBuild: true);  // fail fast if cycles/missing deps
```

Use these flags **in development only**; they add overhead.

## 8. Injection Styles

### 8.1 Constructor Injection (Preferred)

```csharp
public class BillingService(ILogger<BillingService> log, IPayment payment) { ... }
```

*Dependencies are explicit & immutable.*

### 8.2 Method Injection

Used for optional/rare dependencies:

```csharp
public Task HandleAsync(Event e, IServiceProvider sp)
{
    var audit = sp.GetService<IAudit>();
}
```

### 8.3 Property Injection

Not supported by MS.DI out‑of‑the‑box; use third‑party containers if needed.

## 9. Composition Root & Service Locator

- **Composition root** – one place where object graph is built (*Program.cs* in ASP.NET / *Main* in console apps).
- *Acceptable* to call `provider.GetRequiredService<App>()` **once** here.\
  *Avoid*\* scattering `GetService<T>()` in business logic – that’s the Service Locator anti‑pattern.\*

## 10. DI Across Application Types

| App Type         | DI Entry                                                    | Special Notes                            |
| ---------------- | ----------------------------------------------------------- | ---------------------------------------- |
| ASP.NET Core     | `builder.Services` then `app.Services`                      | Scoped = per HTTP request                |
| Console / Worker | `Host.CreateDefaultBuilder()` or manual `ServiceCollection` | Create scopes manually for units‐of‐work |
| WinForms/WPF     | `IHost` inside `Program.cs`                                 | Inject into forms via constructor        |
| Minimal API      | One‑file `Program.cs`; DI available via delegates           | Lambdas can inject params directly       |

## 11. Comparison with Third‑Party Containers

| Feature                                                | MS.DI   | Autofac | SimpleInjector |
| ------------------------------------------------------ | ------- | ------- | -------------- |
| Keyed services                                         | ❌       | ✅       | ✅              |
| Interception/AOP                                       | ❌       | ✅       | 🔸 minimal     |
| Diagnostics GUI                                        | ❌       | 🔸      | ✅              |
| Performance                                            | 🟢 Fast | 🟡      | 🟢             |
| *Choose MS.DI unless an advanced feature is required.* |         |         |                |

## 12. Common Anti‑Patterns & Smells

1. **Captive dependency** – Singleton consumes Scoped.
2. **Mega constructor** – >5 dependencies; extract façade.
3. **Service locator in domain logic** – hides dependencies, kills tests.
4. **Transient storm** – heavy object registered transient & resolved repeatedly.
5. **Manual disposal** of injected services – container should manage lifecycle.

## 13. Quick‑Start Cheatsheet

```csharp
// Register
services.AddSingleton<ISvc, Svc>();
services.AddScoped<DbContext>();
services.AddTransient<Hasher>();

// Build with validation (dev)
var sp = services.BuildServiceProvider(true, true);

// Resolve root
var app = sp.GetRequiredService<App>();
await app.RunAsync();
```

## 14. Further Reading

- Official docs: microsoft.com/dotnet > Dependency injection guide.
- Mark Seemann – *Dependency Injection in .NET* (book).
- Steve Smith – *ASP.NET Core Fundamentals* series.
- **Next manuals:** See *Document 2* for lifetimes, *Document 3* for registration patterns.

---

© 2025 DI Playbook – License: MIT

