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