using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public sealed class SpatialMetadataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SpatialMetadata);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {

            if (reader.TokenType == JsonToken.Null)
                return null;

            JObject jo = JObject.Load(reader);
            JToken typeToken = jo["type"];

            if (typeToken == null)
                return null;

            string type = typeToken.Value<string>();

            SpatialMetadata target;

            switch (type.ToLowerInvariant())
            {
                case "bbox":
                    target = new BBoxSpatialMetadata();
                    break;

                case "point":
                    target = new PointSpatialMetadata();
                    break;

                default:
                    throw new JsonSerializationException(
                        "Unknown spatial_metadata type: " + type);
            }

            serializer.Populate(jo.CreateReader(), target);
            return target;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
