using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.UI.Hooks.Logs
{
    public class EditDatasetDetailsLog
    {
        public List<LogMessage> Messages { get; set; }

        public EditDatasetDetailsLog()
        {
            Messages = new List<LogMessage>(); 
        }
    }
}
