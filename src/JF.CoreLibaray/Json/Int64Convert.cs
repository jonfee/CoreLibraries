using Newtonsoft.Json;
using System;

namespace JF.Json
{
    public class Int64Convert : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var type = Type.GetTypeFromHandle(objectType.TypeHandle);

            return type == typeof(Int64) || type == typeof(Nullable<Int64>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool isNullable = IsNullableType(objectType);

            Type t = IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType;

            if (reader.TokenType == JsonToken.Null) return null;

            try
            {
                long temp = 0;
                Int64.TryParse(reader.Value?.ToString(), out temp);
                return temp;
            }
            catch
            {
                return null;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is Int64 || value is Nullable<Int64>)
            {
                writer.WriteValue(value.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }

        private bool IsNullableType(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }
            return (t.FullName == "System.ValueType" && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }
    }
}
