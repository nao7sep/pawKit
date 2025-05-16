using System.Numerics;

namespace pawKitLib.Settings
{
    public sealed class SettingValue
    {
        public List<string?>? Values { get; }

        public SettingValue(List<string?>? values)
        {
            Values = values;
        }

        public static SettingValue Null() => new(null);
        public static SettingValue Single(string? value) => new([value]);
        public static SettingValue Multiple(IEnumerable<string?> values) => new([..values]);

        // Accessors
        public bool IsNull => Values == null;
        public bool IsSingle => Values is { Count: 1 };
        public bool IsMultiple => Values is { Count: > 1 };
        public string? AsSingleOrNull() => IsSingle ? Values![0] : null;
        public string?[]? AsArrayOrNull() => Values?.ToArray();

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
        public static SettingValue FromBool(bool value) => Single(StringTypeConverter.FromBool(value));
        public static SettingValue FromChar(char value) => Single(StringTypeConverter.FromChar(value));
        public static SettingValue FromSByte(sbyte value) => Single(StringTypeConverter.FromSByte(value));
        public static SettingValue FromShort(short value) => Single(StringTypeConverter.FromShort(value));
        public static SettingValue FromInt(int value) => Single(StringTypeConverter.FromInt(value));
        public static SettingValue FromLong(long value) => Single(StringTypeConverter.FromLong(value));
        public static SettingValue FromBigInteger(BigInteger value) => Single(StringTypeConverter.FromBigInteger(value));
        public static SettingValue FromByte(byte value) => Single(StringTypeConverter.FromByte(value));
        public static SettingValue FromUShort(ushort value) => Single(StringTypeConverter.FromUShort(value));
        public static SettingValue FromUInt(uint value) => Single(StringTypeConverter.FromUInt(value));
        public static SettingValue FromULong(ulong value) => Single(StringTypeConverter.FromULong(value));
        public static SettingValue FromFloat(float value) => Single(StringTypeConverter.FromFloat(value));
        public static SettingValue FromDouble(double value) => Single(StringTypeConverter.FromDouble(value));
        public static SettingValue FromDecimal(decimal value) => Single(StringTypeConverter.FromDecimal(value));
        public static SettingValue FromGuid(Guid value) => Single(StringTypeConverter.FromGuid(value));
        public static SettingValue FromDateTime(DateTime value) => Single(StringTypeConverter.FromDateTime(value));
        public static SettingValue FromDateTimeOffset(DateTimeOffset value) => Single(StringTypeConverter.FromDateTimeOffset(value));
        public static SettingValue FromTimeSpan(TimeSpan value) => Single(StringTypeConverter.FromTimeSpan(value));
        public static SettingValue FromEnum<TEnum>(TEnum value) where TEnum : struct, Enum => Single(StringTypeConverter.FromEnum(value));
        public static SettingValue FromBase64(byte[] value) => Single(StringTypeConverter.FromBase64(value));
        #endregion

        public override string ToString()
        {
            if (IsNull)
                return StringDisplayHelper.DisplayNull();
            if (IsSingle)
                return StringDisplayHelper.DisplayValue(Values![0]);
            return "[" + string.Join(", ", Values!.Select(StringDisplayHelper.DisplayValue)) + "]";
        }
    }
}
