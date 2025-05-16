using System.Text.Json;
using System.Text.Json.Serialization;

namespace pawKitLib.Settings
{
    public class SettingValueJsonConverter : JsonConverter<SettingValue>
    {
        public override SettingValue? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return SettingValue.Null();
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                var list = new List<string?>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;
                    if (reader.TokenType == JsonTokenType.Null)
                        list.Add(null);
                    else if (reader.TokenType == JsonTokenType.String)
                        list.Add(reader.GetString());
                    else
                        throw new JsonException("Unexpected token in SettingValue array");
                }
                return new SettingValue(list);
            }
            if (reader.TokenType == JsonTokenType.String)
                return SettingValue.Single(reader.GetString());
            return SettingValue.Single(null);
        }

        public override void Write(Utf8JsonWriter writer, SettingValue value, JsonSerializerOptions options)
        {
            if (value.Values == null)
            {
                writer.WriteNullValue();
            }
            else if (value.Values.Count == 1)
            {
                if (value.Values[0] == null)
                    writer.WriteNullValue();
                else
                    writer.WriteStringValue(value.Values[0]);
            }
            else
            {
                writer.WriteStartArray();
                foreach (var v in value.Values)
                {
                    if (v == null)
                        writer.WriteNullValue();
                    else
                        writer.WriteStringValue(v);
                }
                writer.WriteEndArray();
            }
        }
    }
}
