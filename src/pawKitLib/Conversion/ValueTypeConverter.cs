using System;
using System.Globalization;
using System.Numerics;

namespace pawKitLib.Conversion
{
    /// <summary>
    /// Provides methods for converting value types (e.g., Guid, DateTime) to and from strings in a consistent, roundtrippable format.
    /// </summary>
    // Note: While supporting all .NET value types may appear to contradict YAGNI (You Aren't Gonna Need It),
    // .NET's standard types are highly stable and unlikely to change. Implementing comprehensive conversion
    // methods now, with the help of AI, is efficient and future-proof in this context.
    // Some developers may wonder why we don't just use the built-in Parse methods directly.
    // The answer: handling culture and format consistently is critical for reliable data exchange.
    // For example, floating point numbers use ',' or '.' depending on locale—this can crash apps if not handled.
    // These helpers avoid subtle bugs and make round-trip conversion safe, no matter the environment.
    public static class ValueTypeConverter
    {
        // Boolean
        public static string ToString(bool value) => value ? "True" : "False";
        public static bool ParseBool(string s) => bool.Parse(s);
        public static bool TryParseBool(string s, out bool value) => bool.TryParse(s, out value);

        // Byte
        public static string ToString(byte value) => value.ToString(CultureInfo.InvariantCulture);
        public static byte ParseByte(string s) => byte.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseByte(string s, out byte value) => byte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        // SByte
        public static string ToString(sbyte value) => value.ToString(CultureInfo.InvariantCulture);
        public static sbyte ParseSByte(string s) => sbyte.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseSByte(string s, out sbyte value) => sbyte.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        // Short
        public static string ToString(short value) => value.ToString(CultureInfo.InvariantCulture);
        public static short ParseShort(string s) => short.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseShort(string s, out short value) => short.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        // UShort
        public static string ToString(ushort value) => value.ToString(CultureInfo.InvariantCulture);
        public static ushort ParseUShort(string s) => ushort.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseUShort(string s, out ushort value) => ushort.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        // Int
        public static string ToString(int value) => value.ToString(CultureInfo.InvariantCulture);
        public static int ParseInt(string s) => int.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseInt(string s, out int value) => int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        // UInt
        public static string ToString(uint value) => value.ToString(CultureInfo.InvariantCulture);
        public static uint ParseUInt(string s) => uint.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseUInt(string s, out uint value) => uint.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        // Long
        public static string ToString(long value) => value.ToString(CultureInfo.InvariantCulture);
        public static long ParseLong(string s) => long.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseLong(string s, out long value) => long.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        // ULong
        public static string ToString(ulong value) => value.ToString(CultureInfo.InvariantCulture);
        public static ulong ParseULong(string s) => ulong.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseULong(string s, out ulong value) => ulong.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        // Float
        public static string ToString(float value) => value.ToString(CultureInfo.InvariantCulture);
        public static float ParseFloat(string s) => float.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseFloat(string s, out float value) => float.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);

        // Double
        public static string ToString(double value) => value.ToString(CultureInfo.InvariantCulture);
        public static double ParseDouble(string s) => double.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseDouble(string s, out double value) => double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);

        // Decimal
        public static string ToString(decimal value) => value.ToString(CultureInfo.InvariantCulture);
        public static decimal ParseDecimal(string s) => decimal.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseDecimal(string s, out decimal value) => decimal.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value);

        // Char
        public static string ToString(char value) => value.ToString();
        public static char ParseChar(string s) => char.Parse(s);
        public static bool TryParseChar(string s, out char value) => char.TryParse(s, out value);

        // Guid
        // Example: d3c1e2a7-8b2e-4c1a-9e2a-1b2e3c4d5f6a
        public static string ToString(Guid value) => value.ToString("D", CultureInfo.InvariantCulture);
        public static Guid ParseGuid(string s) => Guid.Parse(s);
        public static bool TryParseGuid(string s, out Guid value) => Guid.TryParse(s, out value);

        // DateTime
        // Example: 2025-07-14T12:34:56.789Z
        public static string ToString(DateTime utcValue) => utcValue.ToString("O", CultureInfo.InvariantCulture);
        public static DateTime ParseDateTime(string s) => DateTime.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal);
        public static bool TryParseDateTime(string s, out DateTime utcValue) => DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal, out utcValue);

        // DateTimeOffset
        // Example: 2025-07-14T12:34:56.789+00:00
        public static string ToString(DateTimeOffset utcValue) => utcValue.ToString("O", CultureInfo.InvariantCulture);
        public static DateTimeOffset ParseDateTimeOffset(string s) => DateTimeOffset.Parse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal);
        public static bool TryParseDateTimeOffset(string s, out DateTimeOffset utcValue) => DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind | DateTimeStyles.AssumeUniversal, out utcValue);

        // TimeSpan
        // Example: -00:00:01.5000000
        public static string ToString(TimeSpan value) => value.ToString("c", CultureInfo.InvariantCulture);
        public static TimeSpan ParseTimeSpan(string s) => TimeSpan.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseTimeSpan(string s, out TimeSpan value) => TimeSpan.TryParse(s, CultureInfo.InvariantCulture, out value);

        // Enum
        // Example: Monday or 1
        public static string ToString<TEnum>(TEnum value) where TEnum : struct, Enum => value.ToString();
        public static TEnum ParseEnum<TEnum>(string s) where TEnum : struct, Enum => (TEnum)Enum.Parse(typeof(TEnum), s, ignoreCase: true);
        public static bool TryParseEnum<TEnum>(string s, out TEnum value) where TEnum : struct, Enum => Enum.TryParse<TEnum>(s, ignoreCase: true, out value);

        // BigInteger
        // Example: 12345678901234567890
        public static string ToString(BigInteger value) => value.ToString(CultureInfo.InvariantCulture);
        public static BigInteger ParseBigInteger(string s) => BigInteger.Parse(s, CultureInfo.InvariantCulture);
        public static bool TryParseBigInteger(string s, out BigInteger value) => BigInteger.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);

        // Uri
        // Example: https://example.com
        // Note: TryParseUri may return null because Uri is a reference type.
        // ParseUri guarantees a non-null result or throws an exception if the input is invalid.
        public static string ToString(Uri value) => value.ToString();
        public static Uri ParseUri(string s) => new Uri(s, UriKind.RelativeOrAbsolute);
        public static bool TryParseUri(string s, out Uri? value) => Uri.TryCreate(s, UriKind.RelativeOrAbsolute, out value);
    }
}
