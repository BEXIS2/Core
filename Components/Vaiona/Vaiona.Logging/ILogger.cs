using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Logging;

namespace Vaiona.Logging
{
    public interface ILogger
    {
        void LogMethod(MethodLogEntry logEntry);
        void LogData(DataLogEntry logEntry);
        void LogRelation(RelationLogEntry logEntry);
        void LogCustom(CustomLogEntry logEntry);
        void LogCustom(string message);
    }
}
