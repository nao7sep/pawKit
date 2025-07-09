namespace pawKitLib.Abstractions;

/// <summary>
/// Defines a contract for a class that can validate an object of a specific type.
/// </summary>
/// <typeparam name="T">The type of the object to validate.</typeparam>
/// <remarks>
/// This abstraction allows for the decoupling of validation logic from business services and models,
/// promoting the Single Responsibility Principle and improving testability.
/// </remarks>
public interface IValidator<in T>
{
    /// <summary>
    /// Asynchronously validates the specified instance.
    /// </summary>
    /// <param name="instance">The instance to validate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous validation operation. The task result will be empty if validation is successful, or contain validation errors if it fails.</returns>
    /// <remarks>
    /// This method is asynchronous to support validation rules that require I/O operations,
    /// such as checking for uniqueness in a database (e.g., "Is this email address already taken?")
    /// or calling an external service.
    /// </remarks>
    Task<IReadOnlyList<string>> ValidateAsync(T instance, CancellationToken cancellationToken = default);
}