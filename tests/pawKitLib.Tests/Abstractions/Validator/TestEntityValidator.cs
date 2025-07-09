using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using pawKitLib.Abstractions;

namespace pawKitLib.Tests.Abstractions.Validator;

/// <summary>
/// A simple validator for <see cref="TestEntity"/>.
/// </summary>
/// <remarks>
/// This validator checks that the Name property is not null, empty, or whitespace.
/// It is used in tests to demonstrate the validation abstraction and error reporting.
/// </remarks>
public class TestEntityValidator : IValidator<TestEntity>
{
    /// <summary>
    /// Validates the specified <see cref="TestEntity"/> instance.
    /// </summary>
    /// <param name="instance">The entity to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A list of validation error messages, or an empty list if valid.</returns>
    public Task<IReadOnlyList<string>> ValidateAsync(TestEntity instance, CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        // Name must not be null, empty, or whitespace.
        if (string.IsNullOrWhiteSpace(instance.Name))
        {
            errors.Add("Name must not be empty.");
        }
        return Task.FromResult((IReadOnlyList<string>)errors);
    }
}
