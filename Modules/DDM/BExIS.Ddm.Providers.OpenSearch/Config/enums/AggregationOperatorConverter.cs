using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config.enums
{
    public sealed class AggregationOperatorConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(AggregationOperator);
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var value = reader.Value as string;

            if (value == null)
                throw new JsonSerializationException("Aggregation operator is null");

            switch (value)
            {
                case "min/max":
                    return AggregationOperator.MinMax;
                case "avg":
                    return AggregationOperator.Average;
                case "sum":
                    return AggregationOperator.Sum;
                default:
                    throw new JsonSerializationException(
                        "Unknown aggregation operator: " + value);
            }
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            var op = (AggregationOperator)value;

            switch (op)
            {
                case AggregationOperator.MinMax:
                    writer.WriteValue("min/max");
                    break;
                case AggregationOperator.Average:
                    writer.WriteValue("avg");
                    break;
                case AggregationOperator.Sum:
                    writer.WriteValue("sum");
                    break;
                default:
                    throw new JsonSerializationException(
                        "Unsupported aggregation operator: " + op);
            }
        }
    }
}
