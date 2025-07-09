using pawKitLib.Abstractions;

namespace pawKitLib.Utils;

/// <summary>
/// Provides high-level, performance-oriented extension methods for <see cref="IRandomProvider"/>.
/// </summary>
public static class RandomProviderExtensions
{
    /// <summary>
    /// Performs an in-place shuffle of an array using the Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="provider">The random number provider.</param>
    /// <param name="values">The array to shuffle.</param>
    public static void Shuffle<T>(this IRandomProvider provider, T[] values)
    {
        provider.Shuffle(values.AsSpan());
    }

    /// <summary>
    /// Performs an in-place shuffle of a span using the Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the span.</typeparam>
    /// <param name="provider">The random number provider.</param>
    /// <param name="values">The span to shuffle.</param>
    public static void Shuffle<T>(this IRandomProvider provider, Span<T> values)
    {
        var n = values.Length;
        while (n > 1)
        {
            n--;
            var k = provider.GetInt32(n + 1);
            (values[k], values[n]) = (values[n], values[k]);
        }
    }

    /// <summary>
    /// Creates an array populated with items chosen at random from the provided set of choices.
    /// This is also known as sampling with replacement.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="provider">The random number provider.</param>
    /// <param name="choices">The set of items to choose from.</param>
    /// <param name="count">The number of items to generate.</param>
    /// <returns>An array filled with randomly chosen elements.</returns>
    public static T[] GetItems<T>(this IRandomProvider provider, T[] choices, int count)
    {
        return provider.GetItems(choices.AsSpan(), count);
    }

    /// <summary>
    /// Creates an array populated with items chosen at random from the provided set of choices.
    /// This is also known as sampling with replacement.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="provider">The random number provider.</param>
    /// <param name="choices">The set of items to choose from.</param>
    /// <param name="count">The number of items to generate.</param>
    /// <returns>An array filled with randomly chosen elements.</returns>
    public static T[] GetItems<T>(this IRandomProvider provider, ReadOnlySpan<T> choices, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (count == 0)
        {
            return [];
        }

        var result = new T[count];
        provider.GetItems(choices, result.AsSpan());
        return result;
    }

    /// <summary>
    /// Fills the elements of a specified span with items chosen at random from the provided set of choices.
    /// This is also known as sampling with replacement.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="provider">The random number provider.</param>
    /// <param name="choices">The set of items to choose from.</param>
    /// <param name="destination">The span to fill with random choices.</param>
    public static void GetItems<T>(this IRandomProvider provider, ReadOnlySpan<T> choices, Span<T> destination)
    {
        if (choices.IsEmpty)
        {
            throw new ArgumentException("Cannot generate items from an empty collection.", nameof(choices));
        }

        for (var i = 0; i < destination.Length; i++)
        {
            destination[i] = choices[provider.GetInt32(choices.Length)];
        }
    }
}