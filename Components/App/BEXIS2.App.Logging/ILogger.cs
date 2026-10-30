using BEXIS2.DB.Entities.Logging;

namespace BEXIS2.App.Logging
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