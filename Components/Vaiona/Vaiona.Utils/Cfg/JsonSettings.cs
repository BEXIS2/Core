using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vaiona.Utils.Cfg
{
    public class JsonSettings
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public Entry[] Entry { get; set; }
    }

    public class Entry
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public Item[] Item { get; set; }
    }

    public class Item
    {
        public Attribute[] Attribute { get; set; }

        public Attribute GetAttribute(string key)
        {
            if (Attribute!=null && Attribute.Any())
            {
                return Attribute.Where(a => a.Key.ToLower().Equals("placeholder")).FirstOrDefault();
            }

            return null;
        }
    }

    public class Attribute
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
    }
}