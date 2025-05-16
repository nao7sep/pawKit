using System.Numerics;

namespace pawKitLib.KeyValueStore
{
    /// <summary>
    /// Represents a value in the KeyValueStore: can be null, a single string, or a list of strings.
    /// Provides type conversion helpers and developer-friendly APIs.
    /// </summary>
    public sealed class StringValues
    {
        public List<string?>? Values { get; }

        public StringValues(List<string?>? values)
        {
            Values = values;
        }

        public static StringValues CreateNull() => new(null);
        public static StringValues CreateSingle(string? value) => new([value]);
        public static StringValues CreateMultiple(IEnumerable<string?> values) => new([..values]);

        // Accessors
        /// <summary>
        /// True if the underlying list is null (no value at all).
        /// </summary>
        public bool IsNull => Values == null;
        /// <summary>
        /// True if the underlying list exists but contains no values.
        /// </summary>
        public bool IsEmpty => Values is { Count: 0 };
        /// <summary>
        /// True if the underlying list contains exactly one value.
        /// </summary>
        public bool IsSingle => Values is { Count: 1 };
        /// <summary>
        /// True if the underlying list contains more than one value.
        /// </summary>
        public bool IsMultiple => Values is { Count: > 1 };
        /// <summary>
        /// Returns the single value if present, otherwise null.
        /// </summary>
        public string? AsSingleOrNull() => IsSingle ? Values![0] : null;
        /// <summary>
        /// Returns the list of values if present and not null, otherwise null.
        /// This method virtually just returns the underlying Values property.
        /// </summary>
        public List<string?>? AsMultipleOrNull() => Values;

        #region Type conversion helpers (AsXxxOrNull and FromXxx)
        // AsXxxOrNull methods
        public bool? AsBoolOrNull() => IsSingle ? StringTypeConverter.ToBoolOrNull(Values![0]) : null;
        public char? AsCharOrNull() => IsSingle ? StringTypeConverter.ToCharOrNull(Values![0]) : null;
        public sbyte? AsSByteOrNull() => IsSingle ? StringTypeConverter.ToSByteOrNull(Values![0]) : null;
        public short? AsShortOrNull() => IsSingle ? StringTypeConverter.ToShortOrNull(Values![0]) : null;
        public int? AsIntOrNull() => IsSingle ? StringTypeConverter.ToIntOrNull(Values![0]) : null;
        public long? AsLongOrNull() => IsSingle ? StringTypeConverter.ToLongOrNull(Values![0]) : null;
        public BigInteger? AsBigIntegerOrNull() => IsSingle ? StringTypeConverter.ToBigIntegerOrNull(Values![0]) : null;
        public byte? AsByteOrNull() => IsSingle ? StringTypeConverter.ToByteOrNull(Values![0]) : null;
        public ushort? AsUShortOrNull() => IsSingle ? StringTypeConverter.ToUShortOrNull(Values![0]) : null;
        public uint? AsUIntOrNull() => IsSingle ? StringTypeConverter.ToUIntOrNull(Values![0]) : null;
        public ulong? AsULongOrNull() => IsSingle ? StringTypeConverter.ToULongOrNull(Values![0]) : null;
        public float? AsFloatOrNull() => IsSingle ? StringTypeConverter.ToFloatOrNull(Values![0]) : null;
        public double? AsDoubleOrNull() => IsSingle ? StringTypeConverter.ToDoubleOrNull(Values![0]) : null;
        public decimal? AsDecimalOrNull() => IsSingle ? StringTypeConverter.ToDecimalOrNull(Values![0]) : null;
        public Guid? AsGuidOrNull() => IsSingle ? StringTypeConverter.ToGuidOrNull(Values![0]) : null;
        public DateTime? AsDateTimeOrNull() => IsSingle ? StringTypeConverter.ToDateTimeOrNull(Values![0]) : null;
        public DateTimeOffset? AsDateTimeOffsetOrNull() => IsSingle ? StringTypeConverter.ToDateTimeOffsetOrNull(Values![0]) : null;
        public TimeSpan? AsTimeSpanOrNull() => IsSingle ? StringTypeConverter.ToTimeSpanOrNull(Values![0]) : null;
        public TEnum? AsEnumOrNull<TEnum>() where TEnum : struct, Enum => IsSingle ? StringTypeConverter.ToEnumOrNull<TEnum>(Values![0]) : null;
        public byte[]? AsBase64OrNull() => IsSingle ? StringTypeConverter.ToBase64OrNull(Values![0]) : null;

        // FromXxx static methods
        public static StringValues FromBool(bool value) => CreateSingle(StringTypeConverter.FromBool(value));
        public static StringValues FromChar(char value) => CreateSingle(StringTypeConverter.FromChar(value));
        public static StringValues FromSByte(sbyte value) => CreateSingle(StringTypeConverter.FromSByte(value));
        public static StringValues FromShort(short value) => CreateSingle(StringTypeConverter.FromShort(value));
        public static StringValues FromInt(int value) => CreateSingle(StringTypeConverter.FromInt(value));
        public static StringValues FromLong(long value) => CreateSingle(StringTypeConverter.FromLong(value));
        public static StringValues FromBigInteger(BigInteger value) => CreateSingle(StringTypeConverter.FromBigInteger(value));
        public static StringValues FromByte(byte value) => CreateSingle(StringTypeConverter.FromByte(value));
        public static StringValues FromUShort(ushort value) => CreateSingle(StringTypeConverter.FromUShort(value));
        public static StringValues FromUInt(uint value) => CreateSingle(StringTypeConverter.FromUInt(value));
        public static StringValues FromULong(ulong value) => CreateSingle(StringTypeConverter.FromULong(value));
        public static StringValues FromFloat(float value) => CreateSingle(StringTypeConverter.FromFloat(value));
        public static StringValues FromDouble(double value) => CreateSingle(StringTypeConverter.FromDouble(value));
        public static StringValues FromDecimal(decimal value) => CreateSingle(StringTypeConverter.FromDecimal(value));
        public static StringValues FromGuid(Guid value) => CreateSingle(StringTypeConverter.FromGuid(value));
        public static StringValues FromDateTime(DateTime value) => CreateSingle(StringTypeConverter.FromDateTime(value));
        public static StringValues FromDateTimeOffset(DateTimeOffset value) => CreateSingle(StringTypeConverter.FromDateTimeOffset(value));
        public static StringValues FromTimeSpan(TimeSpan value) => CreateSingle(StringTypeConverter.FromTimeSpan(value));
        public static StringValues FromEnum<TEnum>(TEnum value) where TEnum : struct, Enum => CreateSingle(StringTypeConverter.FromEnum(value));
        public static StringValues FromBase64(byte[] value) => CreateSingle(StringTypeConverter.FromBase64(value));
        #endregion
    }
}
