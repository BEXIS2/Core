using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Vaiona.Utils.Cfg
{
    public class JsonSettings
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("entries")]
        public List<Entry> Entries { get; set; }

        public JsonSettings()
        {
            Entries = new List<Entry>();
        }
    }

    public class Entry
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public dynamic Value { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("type")]
        public EntryType Type { get; set; }

        [JsonProperty("options", NullValueHandling=NullValueHandling.Ignore)]
        public List<dynamic> Options { get; set; }

        public Entry()
        {
            Options = new List<dynamic>();
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EntryType
    {
        [EnumMember(Value = "Character")]
        Character = 1,

        [EnumMember(Value = "String")]
        String = 2,

        [EnumMember(Value = "Integer")]
        Integer = 3,

        [EnumMember(Value = "Decimal")]
        Decimal = 4,

        [EnumMember(Value = "JSON")]
        JSON = 5,

        [EnumMember(Value = "Date")]
        Date = 6,

        [EnumMember(Value = "EntryList")]
        EntryList = 7,

        [EnumMember(Value = "Boolean")]
        Boolean = 8,
    }

    //public class Item
    //{
    //    public Attribute[] Attribute { get; set; }

    //    public Attribute GetAttribute(string key)
    //    {
    //        if (Attribute != null && Attribute.Any())
    //        {
    //            return Attribute.Where(a => a.Key.ToLower().Equals(key)).FirstOrDefault();
    //        }

    //        return null;
    //    }
    //}

    //public class Attribute
    //{
    //    public string Key { get; set; }
    //    public object Value { get; set; }
    //    public string Type { get; set; }
    //}
}