using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Utils.Config.Configurations
{
    public class ExceptionConfiguration
    {
        [JsonProperty("sendExceptions")]
        public bool sendExceptions { get; set; }

        [JsonProperty("excludedHttpStatusCodes")]
        public List<int> ExcludedHttpStatusCodes{ get; set; }

        [JsonProperty("excludedMessages")]

        public List<string> ExcludedMessages { get; set; }

        public ExceptionConfiguration()
        {
            sendExceptions = false;
            ExcludedHttpStatusCodes = new List<int>();
            ExcludedMessages = new List<string>();
        }
    }
}
