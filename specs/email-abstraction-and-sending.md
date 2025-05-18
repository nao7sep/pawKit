# Email Abstraction and Sending Specification for pawKitLib

## 1. Overview

This document specifies the requirements and design for **Email Abstraction and Sending** in `pawKitLib`. The goal is to provide a clean, dependency-free interface for email sending, with optional lightweight implementations for development and testing, and to avoid unnecessary dependencies (such as MailKit) in applications that do not require email functionality.

---

## 2. Design Philosophy

- **No Dependency Bloat**: The core library does not reference MailKit, SMTP, or any third-party email libraries.
- **Interface-Driven**: Email sending is abstracted via interfaces, allowing consumers to provide their own implementations.
- **Testability**: Default implementations for console and file output are provided for development and testing.
- **Extensibility**: Applications can add real email providers (e.g., MailKit) via separate adapter projects.

---

## 3. Core Interfaces and Classes

### 3.1. IEmailSender Interface

```csharp
public interface IEmailSender
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
```

### 3.2. EmailMessage Model

```csharp
public class EmailMessage
{
    public string Subject { get; set; }
    public string Body { get; set; }
    public List<string> To { get; set; } = new();
    public List<string> Cc { get; set; } = new();
    public List<string> Bcc { get; set; } = new();
    public string From { get; set; }
    public bool IsHtml { get; set; } = true;
    // Optionally: attachments, metadata, etc.
}
```

### 3.3. Default Implementations

- **ConsoleEmailSender**: Writes email details to the console for development/testing.
- **FileEmailSender**: Writes email details to a file (e.g., for local inspection or test automation).

### 3.4. Integration Points

- **No core dependency**: Real email sending (SMTP, MailKit, etc.) is implemented in separate adapter projects.
- **DI/Configuration**: Applications register the desired implementation via DI or factory methods.

---

## 4. Usage Example

```csharp
// Register ConsoleEmailSender for development
services.AddSingleton<IEmailSender, ConsoleEmailSender>();

// Send an email
await emailSender.SendAsync(new EmailMessage {
    Subject = "Test Email",
    Body = "Hello, world!",
    To = new List<string> { "user@example.com" },
    From = "noreply@example.com"
});
```

---

## 5. File and Namespace Structure

- Namespace: `pawKitLib.Email`
- One class/interface per file; file name matches type name.
- All email-related types are in the `Email` subfolder and namespace.

---

## 6. Rationale and Best Practices

- **Why interface-driven?**
  - Avoids unnecessary dependencies for apps that do not use email.
  - Enables easy testing and mocking.
- **Why default implementations?**
  - Allows for local development and CI testing without sending real emails.
- **Why separate adapters?**
  - Keeps the core library lightweight and portable.

---

**End of Email Abstraction and Sending Specification for pawKitLib**
