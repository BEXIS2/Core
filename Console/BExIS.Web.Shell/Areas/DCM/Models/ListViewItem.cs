
namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class ListViewItem
    {
        public long Id { get; set; }
        public string Title { get; set; }

        public ListViewItem(long id, string title)
        {
            Id = id;
            Title = title;
        }

        public ListViewItem()
        {
            // TODO: Complete member initialization
        }
    }
}