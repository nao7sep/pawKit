using System.Globalization;

namespace pawKitLib
{
    public static class StringTypeConverter
    {
        // ToXxxOrNull methods (parsing from string)

        // Boolean
        public static bool? ToBoolOrNull(string? value)
            => bool.TryParse(value, out var result) ? result : null;

        // Char
        public static char? ToCharOrNull(string? value)
            => char.TryParse(value, out var result) ? result : null;

        // Signed integers
        public static sbyte? ToSByteOrNull(string? value)
            => sbyte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
        public static short? ToShortOrNull(string? value)
            => short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
        public static int? ToIntOrNull(string? value)
            => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
        public static long? ToLongOrNull(string? value)
            => long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
        public static System.Numerics.BigInteger? ToBigIntegerOrNull(string? value)
            => System.Numerics.BigInteger.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;

        // Unsigned integers
        public static byte? ToByteOrNull(string? value)
            => byte.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
        public static ushort? ToUShortOrNull(string? value)
            => ushort.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
        public static uint? ToUIntOrNull(string? value)
            => uint.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;
        public static ulong? ToULongOrNull(string? value)
            => ulong.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result) ? result : null;

        // Floating point
        public static float? ToFloatOrNull(string? value)
            => float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result) ? result : null;
        public static double? ToDoubleOrNull(string? value)
            => double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result) ? result : null;
        public static decimal? ToDecimalOrNull(string? value)
            => decimal.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var result) ? result : null;

        // Special types
        public static Guid? ToGuidOrNull(string? value)
            => Guid.TryParse(value, out var result) ? result : null;
        public static DateTime? ToDateTimeOrNull(string? value)
            => DateTime.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result) ? result : null;
        public static DateTimeOffset? ToDateTimeOffsetOrNull(string? value)
            => DateTimeOffset.TryParseExact(value, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result) ? result : null;
        public static TimeSpan? ToTimeSpanOrNull(string? value)
            => TimeSpan.TryParseExact(value, "c", CultureInfo.InvariantCulture, out var result) ? result : null;
        public static TEnum? ToEnumOrNull<TEnum>(string? value) where TEnum : struct, Enum
            => Enum.TryParse<TEnum>(value, ignoreCase: true, out var result) ? result : null;
        public static byte[]? ToBase64OrNull(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            try
            {
                return Convert.FromBase64String(value);
            }
            catch
            {
                return null;
            }
        }

        // FromXxx methods (converting to string)

        // Boolean
        public static string FromBool(bool value)
            => value.ToString();

        // Char
        public static string FromChar(char value)
            => value.ToString(CultureInfo.InvariantCulture);

        // Signed integers
        public static string FromSByte(sbyte value)
            => value.ToString(CultureInfo.InvariantCulture);
        public static string FromShort(short value)
            => value.ToString(CultureInfo.InvariantCulture);
        public static string FromInt(int value)
            => value.ToString(CultureInfo.InvariantCulture);
        public static string FromLong(long value)
            => value.ToString(CultureInfo.InvariantCulture);
        public static string FromBigInteger(System.Numerics.BigInteger value)
            => value.ToString(CultureInfo.InvariantCulture);

        // Unsigned integers
        public static string FromByte(byte value)
            => value.ToString(CultureInfo.InvariantCulture);
        public static string FromUShort(ushort value)
            => value.ToString(CultureInfo.InvariantCulture);
        public static string FromUInt(uint value)
            => value.ToString(CultureInfo.InvariantCulture);
        public static string FromULong(ulong value)
            => value.ToString(CultureInfo.InvariantCulture);

        // Floating point
        public static string FromFloat(float value)
            => value.ToString(CultureInfo.InvariantCulture);
        public static string FromDouble(double value)
            => value.ToString(CultureInfo.InvariantCulture);
        public static string FromDecimal(decimal value)
            => value.ToString(CultureInfo.InvariantCulture);

        // Special types
        public static string FromGuid(Guid value)
            => value.ToString();
        public static string FromDateTime(DateTime value)
            => value.ToString("O", CultureInfo.InvariantCulture);
        public static string FromDateTimeOffset(DateTimeOffset value)
            => value.ToString("O", CultureInfo.InvariantCulture);
        public static string FromTimeSpan(TimeSpan value)
            => value.ToString("c", CultureInfo.InvariantCulture);
        public static string FromEnum<TEnum>(TEnum value) where TEnum : struct, Enum
            => value.ToString();
        public static string FromBase64(byte[] value)
            => Convert.ToBase64String(value);
    }
}
