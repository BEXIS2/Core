using System.Collections.Generic;

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