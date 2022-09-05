using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.UI.Models
{
    public class ListItem
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string Group { get; set; }

        public ListItem()
        {
            Id = 0;
            Text = "";
            Group = "";
        }

        public ListItem(long key, string value, string group="")
        {
            Id = key;
            Text = value;
            Group = group;
        }

    }
}
