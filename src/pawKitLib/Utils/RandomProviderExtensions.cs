using System.Text;
using pawKitLib.Abstractions;

namespace pawKitLib.Utils;

/// <summary>
/// Provides high-level, performance-oriented extension methods for <see cref="IRandomProvider"/>.
/// </summary>
public static class RandomProviderExtensions
{
    private static readonly char[] AlphanumericChars =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();

    // A character set that excludes visually similar characters like O, 0, I, l, 1.
    private static readonly char[] FriendlyChars =
        "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    // A small set of syllables for generating pronounceable (but fake) words for mock data.
    private static readonly string[] Syllables =
        ["ban", "lek", "mur", "zon", "tal", "vex", "cor", "nis", "wok", "jen", "fal", "pom"];

    // A small, curated list of words for generating memorable passphrases.
    private static readonly string[] PassphraseWords =
    [
        "apple", "staple", "battery", "correct", "horse", "purple", "window", "river", "mountain", "ocean",
        "sunshine", "galaxy", "comet", "robot", "dragon", "wizard", "anchor", "velvet", "marble", "signal"
    ];

    // Character sets for password generation.
    private static readonly char[] PasswordLowercaseChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    private static readonly char[] PasswordUppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private static readonly char[] PasswordDigitChars = "0123456789".ToCharArray();
    private static readonly char[] PasswordSymbolChars = "!@#$%^&*()-_=+<,>.".ToCharArray();

    private static readonly char[] FullPasswordChars =
        [..PasswordLowercaseChars, ..PasswordUppercaseChars, ..PasswordDigitChars, ..PasswordSymbolChars];


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

    /// <summary>
    /// Generates a cryptographically secure random string using a given character set.
    /// This is the foundational method for custom string generation.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <param name="length">The desired length of the string.</param>
    /// <param name="characterSet">The set of characters to choose from.</param>
    /// <returns>A random string of the specified length.</returns>
    /// <remarks>
    /// Use this method when you need full control over the character set, for example, to create a code that excludes vowels to prevent accidental word formation.
    /// This method is cryptographically secure and avoids modulo bias by using the unbiased
    /// <see cref="IRandomProvider.GetInt32(int)"/> method for character selection.
    /// </remarks>
    public static string GetString(this IRandomProvider provider, int length, ReadOnlySpan<char> characterSet)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);
        if (characterSet.IsEmpty)
        {
            throw new ArgumentException("Character set cannot be empty.", nameof(characterSet));
        }

        // The high-performance string.Create method cannot be used here because its lambda
        // cannot capture 'characterSet', which is a ref struct (ReadOnlySpan<char>).
        // The fallback is to create a char array and initialize the string from it.
        // This is a simple, correct, and still highly performant approach.
        var buffer = new char[length];
        for (var i = 0; i < buffer.Length; i++)
        {
            buffer[i] = characterSet[provider.GetInt32(characterSet.Length)];
        }
        return new string(buffer);
    }

    /// <summary>
    /// Generates a random alphanumeric string of a given length.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <param name="length">The desired length of the string.</param>
    /// <returns>A random alphanumeric string.</returns>
    /// <remarks>
    /// Suitable for developer-friendly identifiers, such as correlation IDs or temporary keys.
    /// </remarks>
    public static string GetAlphanumericString(this IRandomProvider provider, int length) =>
        provider.GetString(length, AlphanumericChars);

    /// <summary>
    /// Generates a short, human-friendly alphanumeric string, excluding visually similar characters.
    /// Useful for referral codes or short, readable IDs.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <param name="length">The desired length of the code.</param>
    /// <returns>A random "friendly" string.</returns>
    /// <remarks>
    /// Ideal for user-facing codes like promo codes, invitation codes, or CAPTCHAs where readability is important.
    /// </remarks>
    public static string GetShortCode(this IRandomProvider provider, int length = 8) =>
        provider.GetString(length, FriendlyChars);

    /// <summary>
    /// Generates a hexadecimal string from a specified number of random bytes.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <param name="byteCount">The number of random bytes to convert to a hex string.</param>
    /// <returns>A random hexadecimal string.</returns>
    /// <remarks>
    /// A good choice for secure tokens or encoded binary data where a simple ASCII representation is needed, such as for cryptographic nonces.
    /// </remarks>
    public static string GetHexString(this IRandomProvider provider, int byteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);
        if (byteCount == 0) return string.Empty;

        var bytes = provider.GetBytes(byteCount);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// Generates a secure, random, URL-safe Base64 string from a specified number of random bytes.
    /// Suitable for generating cryptographic tokens or API keys.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <param name="byteCount">The number of random bytes to use as entropy for the secret.</param>
    /// <param name="trimPadding">If true, removes trailing '=' padding characters for a cleaner string. Note that padding may need to be restored for standard decoders.</param>
    /// <returns>A cryptographically secure, URL-safe Base64 string.</returns>
    /// <remarks>
    /// This is the recommended method for generating tokens that will be transmitted in URLs, such as API keys, password reset tokens, or OAuth state parameters.
    /// </remarks>
    public static string GetUrlSafeBase64String(this IRandomProvider provider, int byteCount, bool trimPadding = true)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);
        if (byteCount == 0) return string.Empty;

        var randomBytes = provider.GetBytes(byteCount);
        var base64String = Convert.ToBase64String(randomBytes).Replace('+', '-').Replace('/', '_');

        return trimPadding ? base64String.TrimEnd('=') : base64String;
    }

    /// <summary>
    /// Generates a random, pronounceable, "word-like" identifier for mock data or fun usernames.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <returns>A randomly generated fake word.</returns>
    /// <remarks>
    /// Perfect for generating mock data for unit tests, fuzzing, or creating fun, readable identifiers for non-critical resources.
    /// </remarks>
    public static string GetFakeWord(this IRandomProvider provider) =>
        string.Concat(provider.GetItems(Syllables, provider.GetInt32(2, 4)));

    /// <summary>
    /// Generates a standard Base64 string from a specified number of random bytes.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <param name="byteCount">The number of random bytes to encode.</param>
    /// <returns>A standard Base64 encoded string.</returns>
    /// <remarks>
    /// Use this for encoding binary data or generating secure tokens that do not need to be URL-safe, such as internal session IDs or encryption nonces.
    /// </remarks>
    public static string GetBase64String(this IRandomProvider provider, int byteCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteCount);
        if (byteCount == 0) return string.Empty;

        var randomBytes = provider.GetBytes(byteCount);
        return Convert.ToBase64String(randomBytes);
    }

    /// <summary>
    /// Generates a memorable, multi-word passphrase, suitable for recovery codes.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <param name="wordCount">The number of words to include in the passphrase.</param>
    /// <param name="separator">The character to use to separate the words.</param>
    /// <returns>A random, memorable passphrase.</returns>
    /// <remarks>
    /// Ideal for generating human-readable and memorable secrets, such as "correct-horse-battery-staple" style recovery codes.
    /// </remarks>
    public static string GetPassphrase(this IRandomProvider provider, int wordCount, string separator = "-")
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(wordCount);
        return string.Join(separator, provider.GetItems(PassphraseWords, wordCount));
    }

    /// <summary>
    /// Selects a single random item from a read-only list.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="provider">The random number provider.</param>
    /// <param name="choices">The list of items to choose from.</param>
    /// <returns>A randomly selected item from the list.</returns>
    /// <remarks>
    /// A convenient shortcut for randomly picking one element from a collection.
    /// </remarks>
    public static T GetItem<T>(this IRandomProvider provider, IReadOnlyList<T> choices)
    {
        if (choices.Count == 0)
        {
            throw new ArgumentException("Cannot select an item from an empty collection.", nameof(choices));
        }
        return choices[provider.GetInt32(choices.Count)];
    }

    /// <summary>
    /// Returns a random boolean value.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <param name="probabilityOfTrue">The probability (from 0.0 to 1.0) that the result will be true.</param>
    /// <returns>A random <c>true</c> or <c>false</c> value.</returns>
    /// <remarks>
    /// Useful for randomizing test conditions or for any scenario requiring a weighted coin flip.
    /// </remarks>
    public static bool GetBoolean(this IRandomProvider provider, double probabilityOfTrue = 0.5)
    {
        if (probabilityOfTrue < 0.0 || probabilityOfTrue > 1.0)
        {
            throw new ArgumentOutOfRangeException(nameof(probabilityOfTrue), "Probability must be between 0.0 and 1.0.");
        }
        return provider.GetDouble() < probabilityOfTrue;
    }

    /// <summary>
    /// Selects a random value from a specified enumeration.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumeration. Must be a valid <see cref="Enum"/>.</typeparam>
    /// <param name="provider">The random number provider.</param>
    /// <returns>A randomly selected value from the enumeration.</returns>
    /// <remarks>
    /// Extremely useful for generating test data or initializing objects with a random state.
    /// </remarks>
    public static TEnum GetEnumValue<TEnum>(this IRandomProvider provider) where TEnum : Enum
    {
        var values = Enum.GetValues<TEnum>();
        return provider.GetItem(values);
    }

    /// <summary>
    /// Generates a cryptographically secure random password that meets common complexity requirements.
    /// </summary>
    /// <param name="provider">The random number provider.</param>
    /// <param name="length">The desired length of the password. Must be at least 8.</param>
    /// <returns>A random password string containing uppercase, lowercase, digit, and symbol characters.</returns>
    /// <remarks>
    /// This method is ideal for generating initial passwords for users in a managed system.
    /// It guarantees the inclusion of at least one character from each major character set
    /// (lowercase, uppercase, digits, symbols) and then shuffles the result to ensure randomness.
    /// </remarks>
    public static string GetPasswordString(this IRandomProvider provider, int length = 16)
    {
        if (length < 8)
        {
            throw new ArgumentOutOfRangeException(nameof(length), "Password length must be at least 8 characters to ensure complexity.");
        }

        // This implementation avoids intermediate lists and array copies for better performance.
        var passwordChars = new char[length];
        var span = passwordChars.AsSpan();

        // 1. Start with one of each required character type to guarantee complexity.
        span[0] = provider.GetItem(PasswordLowercaseChars);
        span[1] = provider.GetItem(PasswordUppercaseChars);
        span[2] = provider.GetItem(PasswordDigitChars);
        span[3] = provider.GetItem(PasswordSymbolChars);

        // 2. Fill the rest of the password with characters from the full set.
        if (length > 4)
        {
            provider.GetItems(FullPasswordChars, span[4..]);
        }

        // 3. Shuffle the entire span to ensure the required characters are not always at the start.
        provider.Shuffle(span);

        return new string(passwordChars);
    }
}