
namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class ListViewItem
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public ListViewItem()
        {
            Id = 0;
            Title = "";
            Description = "";
        }

        public ListViewItem(long id, string title, string description="")
        {
            Id = id;
            Title = title;
            Description = description;
        }

        
    }
}