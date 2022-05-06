using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.UI.Models
{
    public class KvP
    {
        public long Id { get; set; }
        public string Text { get; set; }

        public KvP()
        {
            Id = 0;
            Text = "";
        }

        public KvP(long key, string value)
        {
            Id = key;
            Text = value;
        }
    }
}
