using System;
using System.Collections.Generic;

namespace BExIS.UI.Hooks
{
    public class LogMessage
    {
        public DateTime Timestamp { get; set; }
        public List<string> Messages { get; set; }

        public string User { get; set; }
        public string Hook { get; set; }
        public string Event { get; set; }

        public LogMessage()
        {
            Messages = new List<string>();
        }

        public LogMessage(DateTime timestamp, List<string> messages, string user, string hook, string _event)
        {
            Timestamp = timestamp;
            Messages = messages;
            User = user;
            Hook = hook;
            Event = _event;
        }

        public LogMessage(DateTime timestamp, string message, string user, string hook, string _event)
        {
            Timestamp = timestamp;
            Messages = new List<string>() { message };
            User = user;
            Hook = hook;
            Event = _event;
        }
    }
}