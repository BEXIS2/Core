using System;

namespace BEXIS2.UI.Models
{
    public class ListItem
    {
        public long Id { get; set; }

        public string Value { get; set; }

        public string Text { get; set; }

        public string Group { get; set; }

        public string Description { get; set; }

        public ListItem()
        {
            Id = 0;
            Value = string.Empty;
            Text = string.Empty;
            Group = string.Empty;
            Description = string.Empty;
        }

        public ListItem(long id, string text, string group = "", string description = "")
        {
            Id = id;
            Value = id.ToString();
            Text = text;
            Group = group;
            Description = description;
        }
    
        public ListItem(string value, string text, string group = "", string description = "")
        {
            Value = value;
            Text = text;
            Group = group;
            Description = description;
        }
    }
}