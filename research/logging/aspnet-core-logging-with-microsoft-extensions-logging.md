<!-- 2025-03-31T02:53:19Z -->

# ASP.NET Core Logging with Microsoft.Extensions.Logging

This document provides a detailed overview of ASP.NET Core's built-in logging functionalities through the Microsoft.Extensions.Logging package. It covers usage, best practices, and includes illustrative sample C# code.

## Overview

ASP.NET Core integrates a powerful logging framework built around the **ILogger** interface. This framework is available via the **Microsoft.Extensions.Logging** package. It supports multiple logging providers such as Console, Debug, EventSource, and more, allowing for flexible configuration and integration with third-party logging systems.

## Key Features

- **Structured Logging**: The logging API supports structured logging which enables the capture of correlated data, providing enhanced query capabilities.
- **Configurable Logging Levels**: Define log levels (Trace, Debug, Information, Warning, Error, Critical) to filter log entries based on their severity.
- **Multiple Providers**: Out-of-the-box support for Console, Debug, and EventSource providers, among others.
- **Dependency Injection**: ILogger can be injected into your classes via DI, making it easier to log from different layers of your application.

## Best Practices

1. **Use Structured Logging**: Pass parameters as separate objects rather than concatenating strings.
   ```csharp
   _logger.LogInformation("User {UserId} accessed the resource.", userId);
   ```
2. **Avoid Sensitive Information**: Ensure that no sensitive or personal data is logged.
3. **Configure Log Levels Appropriately**: Use the appropriate log level for the type of log entry.
4. **Leverage Dependency Injection**: Always inject ILogger into your classes rather than creating new instances.

## Sample Code

### Configuring Logging in Program.cs

Below is an example of how to configure logging in an ASP.NET Core application:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Clear default providers and add console logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Build the app
var app = builder.Build();

app.MapGet("/", (ILogger<Program> logger) =>
{
    logger.LogInformation("Home route accessed at {Time}", DateTime.UtcNow);
    return "Hello, logging!";
});

app.Run();
```

### Using ILogger in a Controller

Here is an example of injecting and using ILogger in an ASP.NET Core MVC controller:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace YourApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Index page accessed at {Time}", DateTime.UtcNow);
            return View();
        }
    }
}
```

## Configuration via appsettings.json

Logging can also be configured via the `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    },
    "Console": {
      "IncludeScopes": true
    }
  }
}
```

This configuration ensures that logs are captured appropriately based on the defined categories and log levels.

## Conclusion

The logging functionalities provided by ASP.NET Core through the Microsoft.Extensions.Logging package offer a robust solution for capturing and managing application logs. By following best practices such as structured logging and appropriate log level configuration, developers can gain valuable insights into their application's behavior while maintaining performance and security.
