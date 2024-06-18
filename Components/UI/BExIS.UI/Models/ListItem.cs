namespace BExIS.UI.Models
{
    public class ListItem
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }

        public ListItem()
        {
            Id = 0;
            Text = "";
            Group = "";
            Description = "";
        }

        public ListItem(long key, string value, string group = "", string description = "")
        {
            Id = key;
            Text = value;
            Group = group;
            Description = description;
        }

        public ListItem(long key, string value, string group)
        {
            Id = key;
            Text = value;
            Group = group;
            Description = "";
        }
    }
}