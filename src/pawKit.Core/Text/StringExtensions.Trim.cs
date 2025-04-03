namespace pawKit.Core.Text
{
    public static partial class StringExtensions
    {
        public static ReadOnlySpan<char> TrimWhiteSpaceAnd(this ReadOnlySpan<char> span, char trimChar)
        {
            span = TrimStartWhiteSpaceAnd(span, trimChar);
            span = TrimEndWhiteSpaceAnd(span, trimChar);
            return span;
        }

        public static ReadOnlySpan<char> TrimWhiteSpaceAnd(this ReadOnlySpan<char> span, params char[] trimChars)
        {
            if (trimChars == null)
                throw new ArgumentNullException(nameof(trimChars));

            if (trimChars.Length == 0)
                return span.Trim();

            span = TrimStartWhiteSpaceAnd(span, trimChars);
            span = TrimEndWhiteSpaceAnd(span, trimChars);
            return span;
        }

        public static string TrimWhiteSpaceAnd(this string value, char trimChar)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return TrimWhiteSpaceAnd(value.AsSpan(), trimChar).ToString();
        }

        public static string TrimWhiteSpaceAnd(this string value, params char[] trimChars)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return TrimWhiteSpaceAnd(value.AsSpan(), trimChars).ToString();
        }

        public static ReadOnlySpan<char> TrimStartWhiteSpaceAnd(this ReadOnlySpan<char> span, char trimChar)
        {
            int start = 0;

            while (start < span.Length)
            {
                char character = span[start];

                if (!(char.IsWhiteSpace(character) || character == trimChar))
                    break;

                start++;
            }

            return span.Slice(start);
        }

        public static ReadOnlySpan<char> TrimStartWhiteSpaceAnd(this ReadOnlySpan<char> span, params char[] trimChars)
        {
            if (trimChars == null)
                throw new ArgumentNullException(nameof(trimChars));

            if (trimChars.Length == 0)
                return span.TrimStart();

            int start = 0;

            while (start < span.Length)
            {
                char character = span[start];

                if (!(char.IsWhiteSpace(character) || Array.IndexOf(trimChars, character) >= 0))
                    break;

                start++;
            }

            return span.Slice(start);
        }

        public static string TrimStartWhiteSpaceAnd(this string value, char trimChar)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return TrimStartWhiteSpaceAnd(value.AsSpan(), trimChar).ToString();
        }

        public static string TrimStartWhiteSpaceAnd(this string value, params char[] trimChars)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return TrimStartWhiteSpaceAnd(value.AsSpan(), trimChars).ToString();
        }

        public static ReadOnlySpan<char> TrimEndWhiteSpaceAnd(this ReadOnlySpan<char> span, char trimChar)
        {
            int end = span.Length - 1;

            while (end >= 0)
            {
                char character = span[end];

                if (!(char.IsWhiteSpace(character) || character == trimChar))
                    break;

                end--;
            }

            return span.Slice(0, end + 1);
        }

        public static ReadOnlySpan<char> TrimEndWhiteSpaceAnd(this ReadOnlySpan<char> span, params char[] trimChars)
        {
            if (trimChars == null)
                throw new ArgumentNullException(nameof(trimChars));

            if (trimChars.Length == 0)
                return span.TrimEnd();

            int end = span.Length - 1;

            while (end >= 0)
            {
                char character = span[end];

                if (!(char.IsWhiteSpace(character) || Array.IndexOf(trimChars, character) >= 0))
                    break;

                end--;
            }

            return span.Slice(0, end + 1);
        }

        public static string TrimEndWhiteSpaceAnd(this string value, char trimChar)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return TrimEndWhiteSpaceAnd(value.AsSpan(), trimChar).ToString();
        }

        public static string TrimEndWhiteSpaceAnd(this string value, params char[] trimChars)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return TrimEndWhiteSpaceAnd(value.AsSpan(), trimChars).ToString();
        }
    }
}