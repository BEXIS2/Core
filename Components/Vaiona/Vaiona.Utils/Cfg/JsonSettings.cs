using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Vaiona.Utils.Cfg
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntryType
    {
        [EnumMember(Value = "Character")]
        Character = 1,

        [EnumMember(Value = "String")]
        String = 2,

        [EnumMember(Value = "Int16")]
        Int16 = 3,

        [EnumMember(Value = "Int32")]
        Int32 = 4,

        [EnumMember(Value = "Int64")]
        Int64 = 5,

        [EnumMember(Value = "Decimal")]
        Decimal = 6,

        [EnumMember(Value = "Single")]
        Single = 7,

        [EnumMember(Value = "Double")]
        Double = 8,

        [EnumMember(Value = "JSON")]
        JSON = 9,

        [EnumMember(Value = "Date")]
        Date = 10,

        [EnumMember(Value = "EntryList")]
        EntryList = 11,

        [EnumMember(Value = "Boolean")]
        Boolean = 12
    }

    public class Entry
    {
        public Entry()
        {
            Options = new List<dynamic>();
        }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public List<dynamic> Options { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("type")]
        public EntryType Type { get; set; }

        [JsonProperty("value")]
        public dynamic Value { get; set; }
    }

    public class JsonSettings
    {
        public JsonSettings()
        {
            Entries = new List<Entry>();
        }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}