# Registration Patterns & Resolution Techniques

> **Document 3 of 8 – Mastering how to map abstractions to implementations and retrieve them correctly**

---

## Table of Contents

1. [Overview](#1-overview)
2. [Core Registration APIs](#2-core-registration-apis)
   1. AddTransient / AddScoped / AddSingleton
   2. Service type vs implementation type vs factory overloads
   3. Open generics (`IRepository<T>`)
3. [Idempotent & Conditional Registration](#3-idempotent--conditional-registration)
   1. TryAdd & TryAddEnumerable
   2. Replace / RemoveAll
   3. Environment‑based switches (`IHostEnvironment`)
4. [Factory Registrations](#4-factory-registrations)
   1. Lambda delegates (`sp => …`)
   2. Using ActivatorUtilities & runtime parameters
5. [Re‑Mapping to Existing Singletons](#5-re-mapping-to-existing-singletons)
   1. Avoiding duplicate instances
   2. Mapping pattern (`AddSingleton<IFoo>(sp => sp.GetRequiredService<Foo>())`)
6. [Multiple Implementations per Interface](#6-multiple-implementations-per-interface)
   1. Registering duplicates → `IEnumerable<T>` injection
   2. Maintaining order & priorities
   3. Filtering (`OfType<TImpl>()`, LINQ)
7. [Named / Keyed Resolution Strategies](#7-named--keyed-resolution-strategies)
   1. DIY selector (dictionary pattern)
   2. Options‑based mapping (config‑driven)
   3. Third‑party container features (Autofac keyed services)
8. [Decorator & Wrapper Patterns](#8-decorator--wrapper-patterns)
   1. Manual decorator registration order
   2. Scrutor helper library
9. [Service Locator: Controlled Usage](#9-service-locator-controlled-usage)
10. [Diagnostics & Validation](#10-diagnostics--validation)
11. [Registration Anti‑Patterns](#11-registration-anti-patterns)
12. [Cheatsheet Snippets](#12-cheatsheet-snippets)
13. [Further Reading](#13-further-reading)

---

## 1. Overview

Correct registration is **step 1** to a healthy DI graph. This document shows every supported overload, how to keep registrations deterministic and conflict‑free, and how to solve advanced scenarios like multiple implementations or runtime selection.

## 2. Core Registration APIs

### 2.1 AddTransient / AddScoped / AddSingleton

```csharp
services.AddTransient<IMailer, SmtpMailer>();    // new each resolve
services.AddScoped<IUnitOfWork, EfUnitOfWork>(); // per scope
services.AddSingleton<ICache, MemoryCache>();    // app‑wide
```

*Each has three overload families:*

1. **Service + Implementation** (`AddScoped<IFoo, Foo>()`)
2. **Concrete only** (`AddScoped<Foo>()`)
3. **Factory delegate** (`AddScoped<IFoo>(sp => new Foo(...))`)

### 2.2 Open Generics

```csharp
services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
```

All closed generic combinations (e.g. `IRepository<Order>`) will resolve.

---

## 3. Idempotent & Conditional Registration

### 3.1 TryAdd / TryAddEnumerable

```csharp
services.TryAddSingleton<IDateTime, SystemClock>(); // only if not registered
services.TryAddEnumerable(
    ServiceDescriptor.Singleton<IFilter, AuthFilter>());
```

*Essential for library authors who want to provide defaults while allowing overrides.*

### 3.2 Replace & RemoveAll

```csharp
services.Replace(ServiceDescriptor.Singleton<ILog, DevLogger>());
services.RemoveAll<ILog>();
```

Useful in **tests** or **environment‑specific** builds.

### 3.3 Environment Switch

```csharp
if (env.IsDevelopment())
    services.AddSingleton<IPayment, FakePayment>();
else
    services.AddSingleton<IPayment, StripePayment>();
```

---

## 4. Factory Registrations

### 4.1 Delegate Factories

```csharp
services.AddScoped<IEmailSender>(sp =>
{
    var cfg = sp.GetRequiredService<IOptions<EmailCfg>>().Value;
    return new SmtpSender(cfg.Host, cfg.Port);
});
```

### 4.2 ActivatorUtilities for Extra Args

```csharp
public class Worker(ILogger<Worker> log, IClock clock, string jobId) { ... }

var worker = ActivatorUtilities.CreateInstance<Worker>(sp, jobId);
```

---

## 5. Re‑Mapping to Existing Singletons

Avoid duplicate singleton instances:

```csharp
services.AddSingleton<Foo>();
services.AddSingleton<IFoo>(sp => sp.GetRequiredService<Foo>());
```

*Both **`Foo`** and **`IFoo`** resolve to the ****same object****.*

---

## 6. Multiple Implementations per Interface

```csharp
services.AddSingleton<ISink, ConsoleSink>();
services.AddSingleton<ISink, FileSink>();
```

### 6.1 Resolving All

```csharp
public Logger(IEnumerable<ISink> sinks) {
    foreach (var s in sinks) s.Write("init");
}
```

### 6.2 Ordering & Priorities

Registration order is preserved in `IEnumerable<T>`.  Use LINQ for custom sort.

---

## 7. Named / Keyed Resolution Strategies

### 7.1 Dictionary Selector Pattern

```csharp
public class SinkSelector(IEnumerable<ISink> sinks)
{
    private readonly Dictionary<string, ISink> _map = sinks.ToDictionary(s => s.GetType().Name);
    public ISink Get(string name) => _map[name];
}
```

### 7.2 Config‑Driven Selection

Inject config + `IOptions` to choose which implementation to use at runtime.

### 7.3 Third‑Party Containers

Autofac’s `Keyed<T>` or SimpleInjector’s `Collection` + `Append` give richer semantics.

---

## 8. Decorator & Wrapper Patterns

### 8.1 Manual Decorator

```csharp
services.AddSingleton<IRepo, EfRepo>();
services.Decorate<IRepo, CachingRepo>(); // via Scrutor
```

### 8.2 Scrutor Helper

```csharp
services.Scan(scan => scan
    .FromAssemblyOf<IRepo>()
    .AddClasses(c => c.AssignableTo<IRepo>())
    .As<IRepo>()
    .WithScopedLifetime());
```

---

## 9. Service Locator: Controlled Usage

*Use **`GetService<T>()`** only in composition roots, test helpers, or factories.*\
Anywhere else → rethink design.

---

## 10. Diagnostics & Validation

- Use `validateOnBuild` + `validateScopes` (Document 2) to catch lifetime issues.
- `services.Any(d => d.ServiceType == typeof(IFoo))` for assertions in tests.

---

## 11. Registration Anti‑Patterns

1. **Duplicate singletons** → two instances when you expected one.
2. **Missing lifetime consistency** – decorator registered transient wrapping singleton.
3. **Wildcard registration via Assembly Scan without filters** – surprise inclusions.
4. **Keyed DIY pattern but forgetting default mapping** – runtime `KeyNotFoundException`.

---

## 12. Cheatsheet Snippets

```csharp
// Map interface ➜ concrete instance
services.AddSingleton<Bar>();
services.AddSingleton<IBar>(sp => sp.GetRequiredService<Bar>());

// Library default that user can override
services.TryAddSingleton<ICache, MemoryCache>();

// Register open generic
services.AddScoped(typeof(IRepo<>), typeof(SqlRepo<>));

// Decorate (Scrutor)
services.Decorate<IRepo, TimedRepo>();
```

---

## 13. Further Reading

- Andrew Lock – *Using TryAdd and Scrutor*
- Autofac docs – *Metadata & keyed services*
- Scrutor GitHub – decorator and assembly scanning examples

---

© 2025 DI Playbook – License: MIT

