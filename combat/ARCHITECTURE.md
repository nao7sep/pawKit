# Architectural Patterns

*This document outlines the correct architectural patterns for consuming the `pawKitLib` library, particularly in stateful UI applications. These are not suggestions.*

## The Immutable Domain Model

The core data types in `pawKitLib.Ai.Sessions`, specifically `AiSession` and `AiMessage`, are **immutable records**. This is a deliberate design choice to ensure predictable state management, thread safety, and the absence of side effects.

This pattern applies specifically to objects whose purpose is to carry **state** across boundaries (DTOs, events, messages). It does not apply to objects whose purpose is to encapsulate **behavior** (services, repositories, controllers), which should be standard `class` types and managed by a dependency injection container.

An `AiSession` object is a **snapshot** of the conversation at a single point in time. You do not "change" a session. You create a *new* session that represents the updated state.

**Wrong:**
```csharp
// This is conceptually and literally impossible.
mySession.Messages.Add(newMessage);
```

**Right:**
```csharp
// Create a new session instance with the new message added to the list.
var updatedSession = mySession with { Messages = mySession.Messages.Add(newMessage) };
```

## The ViewModel Adapter Pattern for UI

A common point of failure is attempting to bind a UI directly to the immutable domain model. This is architecturally incompetent and will lead to a non-functional, inefficient application.

**The Problem:** UI frameworks like WPF, Avalonia, or MAUI (using the MVVM pattern) rely on mutable ViewModels and `INotifyPropertyChanged` for efficient two-way data binding. An immutable `AiSession` is fundamentally incompatible with this.

**The Solution:** You MUST use a **ViewModel Adapter**. This is a mutable class in your UI project that represents the state of a single message *for the view*.

### Example: `MessageViewModel`

This class lives in your UI application, **not** in `pawKitLib`.

```csharp
// This is a mutable class for your UI.
public class MessageViewModel : INotifyPropertyChanged
{
    // A stable reference to the domain object's ID
    public Guid MessageId { get; }

    // Properties for display
    public string Author { get; }
    public string Content { get; }

    // UI-specific, mutable state
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            // Standard INotifyPropertyChanged implementation
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    // Constructor maps from the immutable model to the mutable ViewModel
    public MessageViewModel(AiMessage message)
    {
        this.MessageId = message.Id;
        this.Author = message.Role.ToString();
        this.Content = (message.Parts.FirstOrDefault(p => p is TextContentPart) as TextContentPart)?.Text ?? "[Non-text content]";
        this.IsSelected = true; // Default UI state
    }

    // ... INotifyPropertyChanged implementation ...
}
```

### Transactional Updates

UI interactions (like clicking a checkbox) only modify the state of the lightweight `MessageViewModel` objects. The `AiSession` is **not touched**.

When a significant event occurs (e.g., the user clicks "Send"), you then perform a transactional update:

1.  Read the current, pristine `AiSession`.
2.  Build a new `ImmutableDictionary<Guid, MessageContextOverride>` on the fly by inspecting the state of your `MessageViewModel` list.
3.  Create a **single**, new `AiSession` snapshot for the request: `currentSession with { ContextOverrides = ... }`.
4.  Pass this new snapshot to the AI client.

This pattern provides a clean separation of concerns, ensuring the UI is fast and responsive while the domain model remains pure and immutable.