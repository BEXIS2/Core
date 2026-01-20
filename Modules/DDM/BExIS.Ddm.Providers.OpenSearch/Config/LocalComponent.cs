using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Ddm.Providers.OpenSearch.Config.enums;
using Newtonsoft.Json;

namespace BExIS.Ddm.Providers.OpenSearch.Config
{
    public class LocalComponent
    {
        [JsonProperty("global_id")]
        public int GlobalId { get; set; }
        [JsonProperty("data_type_id")]
        [JsonConverter(typeof(StringToEnumConverter<DataTypeId>))]
        public DataTypeId DataTypeId { get; set; }
        [JsonProperty("metadata_nodes")]
        public List<string> MetadataNodes { get; set; } = new List<string>();
    }
}
