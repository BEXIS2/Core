using System.Collections.Generic;

namespace BEXIS2.UI.Hooks.Logs
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