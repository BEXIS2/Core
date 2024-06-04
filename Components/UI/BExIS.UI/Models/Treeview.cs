using System.Collections.Generic;

namespace BExIS.UI.Models
{
    public class Treeview
    {
        public bool showcheckbox { get; set; }
        public bool showcount { get; set; }
        public string target { get; set; }

        public List<TreeviewItem> data { get; set; }

        public Treeview()
        {
            showcheckbox = false;
            showcount = false;
            target = "_blank";

            data = new List<TreeviewItem>();
        }
    }

    public class TreeviewItem
    {
        public string label { get; set; }
        public string descripton { get; set; }
        public int count { get; set; }
        public string value { get; set; }
        public bool active { get; set; }

        public List<TreeviewItem> items { get; set; }

        public TreeviewItem()
        {
            items = new List<TreeviewItem>();
        }
    }
}