using System.Collections.Generic;

namespace BEXIS2.UI.Models
{
    public class SelectList : List<ListItem>
    {
        public SelectList()
        {
        }

        public SelectList(IEnumerable<ListItem> items)
            : base(items)
        {
        }
    }
}