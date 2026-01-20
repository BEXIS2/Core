using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config.enums
{
    public sealed class StringToEnumConverter<TEnum> : JsonConverter
        where TEnum : struct
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TEnum);
        }
        private static readonly Dictionary<string, DataTypeId> _aliasMap = new Dictionary<string, DataTypeId>(StringComparer.OrdinalIgnoreCase)
        {
            { "int", DataTypeId.Integer },
            { "float", DataTypeId.Float },
            { "bool", DataTypeId.Boolean },
            { "text", DataTypeId.Text },
            { "long", DataTypeId.Long },
            { "string", DataTypeId.Text },
            { "double", DataTypeId.Double },

            // weitere Abkürzungen hier
        };

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var value = reader.Value as string;
            if (value == null)
                throw new JsonSerializationException("Enum value is null");

            value = value
                .Replace(":", "_")
                .Replace("/", "_")
                .Replace("-", "_")
                .Replace(" ", "_");

            if (_aliasMap.TryGetValue(value, out var mappedValue))
                return mappedValue;

            TEnum result;
            if (Enum.TryParse(value, true, out result))
                return result;

            throw new JsonSerializationException(
                "Invalid value '" + reader.Value + "' for enum " + typeof(TEnum).Name);
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }
    }

}
