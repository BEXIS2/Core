using System;
using System.IO;
using Vaiona.Entities.Logging;
using Vaiona.Utils.Cfg;

namespace Vaiona.Logging.Loggers
{
    public class FileLogger : ILogger
    {
        //private static FileLogger logger = new FileLogger();
        private FileLogger()
        {
        }

        public static FileLogger GetInstance()
        {
            return new FileLogger();// logger;
        }

        private string buildLogFileName()
        {
            string serialNo = string.Format("{0}.{1}.{2}", DateTime.UtcNow.Year.ToString("D4"), DateTime.UtcNow.Month.ToString("D2"), DateTime.UtcNow.Day.ToString("D2"));
            string fileName = "bexis." + serialNo + ".log";
            string logFolder = Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "Logging");
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            string logFile = Path.Combine(logFolder, fileName);
            return logFile;
        }

        public void LogCustom(CustomLogEntry logEntry)
        {
            throw new NotImplementedException();
        }

        // This mechanism must be replaced with a robust solution. It is only experimental.
        public void LogCustom(string message)
        {
            // get file name
            string logFile = buildLogFileName();

            // prepare message
            string wrappedMessage = string.Format("{0}: {1}", DateTime.UtcNow, message);

            // append to file
            try
            {
                using (StreamWriter file = new StreamWriter(logFile, true))
                {
                    file.WriteLine(wrappedMessage);
                }
            }
            catch { }
        }

        public void LogData(DataLogEntry logEntry)
        {
            throw new NotImplementedException();
        }

        public void LogMethod(MethodLogEntry logEntry)
        {
            throw new NotImplementedException();
        }

        public void LogRelation(RelationLogEntry logEntry)
        {
            throw new NotImplementedException();
        }
    }
}