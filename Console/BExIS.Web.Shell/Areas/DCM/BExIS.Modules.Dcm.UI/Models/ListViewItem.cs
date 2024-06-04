namespace BExIS.Modules.Dcm.UI.Models
{
    public class ListViewItem
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public ListViewItem()
        {
            Id = 0;
            Title = "";
            Description = "";
        }

        public ListViewItem(long id, string title, string description = "")
        {
            Id = id;
            Title = title;
            Description = description;
            DisplayName = "(" + id + ") " + title;
        }
    }

    public class ListViewItemWithType : ListViewItem
    {
        public string Type { get; set; }

        public ListViewItemWithType()
        {
            Type = "No Type";
        }

        public ListViewItemWithType(long id, string title, string description = "", string type = "")
        {
            Id = id;
            Title = title;
            Description = description;
            Type = type;
        }
    }
}