using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BExIS.Ddm.Providers.OpenSearch.Config.converter
{
    public class PrimaryDataAggregationConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<PrimaryDataAggregation>);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Array)
            {
                return token.ToObject<List<PrimaryDataAggregation>>(serializer);
            }

            if (token.Type == JTokenType.Object)
            {
                var single = token.ToObject<PrimaryDataAggregation>(serializer);
                return new List<PrimaryDataAggregation> { single };
            }

            throw new JsonSerializationException("Invalid calc format");
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
