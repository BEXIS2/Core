using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.UI.Models
{
    public  class ExtensionItem
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }

        public string LinkType { get; set; }
        public string ReferenceType { get; set; }
    }
}
