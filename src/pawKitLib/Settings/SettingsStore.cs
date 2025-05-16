using System.Numerics;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace pawKitLib.Settings
{
    /// <summary>
    /// SettingsStore manages a case-insensitive dictionary of settings.
    /// Keys are case-insensitive and null keys are not permitted.
    /// </summary>
    public class SettingsStore
    {
        private readonly Dictionary<string, SettingValue> _settings;
        public IReadOnlyDictionary<string, SettingValue> Settings => _settings;

        private readonly ReaderWriterLockSlim? _lock;
        private readonly bool _threadSafe;

        public SettingsStore(bool threadSafe = true)
        {
            _settings = new Dictionary<string, SettingValue>(StringComparer.OrdinalIgnoreCase);
            _threadSafe = threadSafe;
            _lock = threadSafe ? new ReaderWriterLockSlim() : null;
        }

        public SettingValue? Get(string key)
        {
            if (_threadSafe)
            {
                _lock!.EnterReadLock();
                try
                {
                    return key is null ? null : _settings.TryGetValue(key, out var value) ? value : null;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            else
            {
                return key is null ? null : _settings.TryGetValue(key, out var value) ? value : null;
            }
        }

        public void Set(string key, SettingValue value)
        {
            if (key is null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (_threadSafe)
            {
                _lock!.EnterWriteLock();
                try
                {
                    _settings[key] = value;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            else
            {
                _settings[key] = value;
            }
        }

        public bool Remove(string key)
        {
            if (_threadSafe)
            {
                _lock!.EnterWriteLock();
                try
                {
                    return key != null && _settings.Remove(key);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            else
            {
                return key != null && _settings.Remove(key);
            }
        }

        public void Clear()
        {
            if (_threadSafe)
            {
                _lock!.EnterWriteLock();
                try
                {
                    _settings.Clear();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            else
            {
                _settings.Clear();
            }
        }

        public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;
        public static JsonSerializerOptions DefaultJsonDeserializeOptions { get; set; } = new JsonSerializerOptions
        {
            // PropertyNameCaseInsensitive is not set because the settings dictionary is already case-insensitive.
        };
        public static JsonSerializerOptions DefaultJsonSerializeOptions { get; set; } = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public void LoadFromFile(string path, Encoding? encoding = null, JsonSerializerOptions? options = null)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Settings file not found: {path}", path);
            }
            encoding ??= DefaultEncoding;
            options ??= DefaultJsonDeserializeOptions;
            options.Converters.Add(new SettingValueJsonConverter());
            var json = File.ReadAllText(path, encoding);
            var dict = JsonSerializer.Deserialize<Dictionary<string, SettingValue>>(json, options);
            if (_threadSafe)
            {
                _lock!.EnterWriteLock();
                try
                {
                    _settings.Clear();
                    if (dict != null)
                    {
                        foreach (var kvp in dict)
                            _settings[kvp.Key] = kvp.Value;
                    }
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            else
            {
                _settings.Clear();
                if (dict != null)
                {
                    foreach (var kvp in dict)
                        _settings[kvp.Key] = kvp.Value;
                }
            }
        }

        public void SaveToFile(string path, Encoding? encoding = null, JsonSerializerOptions? options = null)
        {
            encoding ??= DefaultEncoding;
            options ??= DefaultJsonSerializeOptions;
            options.Converters.Add(new SettingValueJsonConverter());
            string json;
            if (_threadSafe)
            {
                _lock!.EnterReadLock();
                try
                {
                    json = JsonSerializer.Serialize(_settings, options);
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            else
            {
                json = JsonSerializer.Serialize(_settings, options);
            }
            File.WriteAllText(path, json, encoding);
        }

        #region Type conversion helpers for direct access by key
        // Getters
        public bool? GetBoolOrNull(string key) => Get(key)?.AsBoolOrNull();
        public char? GetCharOrNull(string key) => Get(key)?.AsCharOrNull();
        public sbyte? GetSByteOrNull(string key) => Get(key)?.AsSByteOrNull();
        public short? GetShortOrNull(string key) => Get(key)?.AsShortOrNull();
        public int? GetIntOrNull(string key) => Get(key)?.AsIntOrNull();
        public long? GetLongOrNull(string key) => Get(key)?.AsLongOrNull();
        public BigInteger? GetBigIntegerOrNull(string key) => Get(key)?.AsBigIntegerOrNull();
        public byte? GetByteOrNull(string key) => Get(key)?.AsByteOrNull();
        public ushort? GetUShortOrNull(string key) => Get(key)?.AsUShortOrNull();
        public uint? GetUIntOrNull(string key) => Get(key)?.AsUIntOrNull();
        public ulong? GetULongOrNull(string key) => Get(key)?.AsULongOrNull();
        public float? GetFloatOrNull(string key) => Get(key)?.AsFloatOrNull();
        public double? GetDoubleOrNull(string key) => Get(key)?.AsDoubleOrNull();
        public decimal? GetDecimalOrNull(string key) => Get(key)?.AsDecimalOrNull();
        public Guid? GetGuidOrNull(string key) => Get(key)?.AsGuidOrNull();
        public DateTime? GetDateTimeOrNull(string key) => Get(key)?.AsDateTimeOrNull();
        public DateTimeOffset? GetDateTimeOffsetOrNull(string key) => Get(key)?.AsDateTimeOffsetOrNull();
        public TimeSpan? GetTimeSpanOrNull(string key) => Get(key)?.AsTimeSpanOrNull();
        public TEnum? GetEnumOrNull<TEnum>(string key) where TEnum : struct, Enum => Get(key)?.AsEnumOrNull<TEnum>();
        public byte[]? GetBase64OrNull(string key) => Get(key)?.AsBase64OrNull();

        // Setters
        public void SetBool(string key, bool value) => Set(key, SettingValue.FromBool(value));
        public void SetChar(string key, char value) => Set(key, SettingValue.FromChar(value));
        public void SetSByte(string key, sbyte value) => Set(key, SettingValue.FromSByte(value));
        public void SetShort(string key, short value) => Set(key, SettingValue.FromShort(value));
        public void SetInt(string key, int value) => Set(key, SettingValue.FromInt(value));
        public void SetLong(string key, long value) => Set(key, SettingValue.FromLong(value));
        public void SetBigInteger(string key, BigInteger value) => Set(key, SettingValue.FromBigInteger(value));
        public void SetByte(string key, byte value) => Set(key, SettingValue.FromByte(value));
        public void SetUShort(string key, ushort value) => Set(key, SettingValue.FromUShort(value));
        public void SetUInt(string key, uint value) => Set(key, SettingValue.FromUInt(value));
        public void SetULong(string key, ulong value) => Set(key, SettingValue.FromULong(value));
        public void SetFloat(string key, float value) => Set(key, SettingValue.FromFloat(value));
        public void SetDouble(string key, double value) => Set(key, SettingValue.FromDouble(value));
        public void SetDecimal(string key, decimal value) => Set(key, SettingValue.FromDecimal(value));
        public void SetGuid(string key, Guid value) => Set(key, SettingValue.FromGuid(value));
        public void SetDateTime(string key, DateTime value) => Set(key, SettingValue.FromDateTime(value));
        public void SetDateTimeOffset(string key, DateTimeOffset value) => Set(key, SettingValue.FromDateTimeOffset(value));
        public void SetTimeSpan(string key, TimeSpan value) => Set(key, SettingValue.FromTimeSpan(value));
        public void SetEnum<TEnum>(string key, TEnum value) where TEnum : struct, Enum => Set(key, SettingValue.FromEnum(value));
        public void SetBase64(string key, byte[] value) => Set(key, SettingValue.FromBase64(value));
        #endregion
    }
}
