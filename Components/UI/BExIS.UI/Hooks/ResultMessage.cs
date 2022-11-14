using System;
using System.Collections.Generic;

namespace BExIS.UI.Hooks
{
    public class ResultMessage
    {
        public DateTime Timestamp { get; set; }
        public List<string> Messages { get; set; }

        public ResultMessage()
        {
        }

        public ResultMessage(DateTime timestamp, List<string> messages)
        {
            Timestamp = timestamp;
            Messages = messages;
        }

        public ResultMessage(DateTime timestamp, string message)
        {
            Timestamp = timestamp;
            Messages = new List<string>() { message };
        }
    }
}