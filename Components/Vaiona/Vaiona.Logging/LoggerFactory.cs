using System;
using System.Diagnostics;
using Vaiona.Entities.Logging;
using Vaiona.Logging.Loggers;
using Vaiona.Utils.Cfg;

namespace Vaiona.Logging
{
    public class LoggerFactory
    {
        public static ILogger GetFileLogger()
        {
            return FileLogger.GetInstance();
        }

        /// <summary>
        /// based on configuration info and provided logType, choose one of the concrete loggers,
        /// search for the specific logType (e.g., Performance.Logging), then for General (General.Logging), and then for the no named registration in the IoC
        /// object creation and lifetime is managed by the IoC container. No need to keep a singleton or static object reference here.
        /// </summary>
        /// <param name="LogType"></param>
        /// <returns></returns>
        private static ILogger create(LogType LogType, string loggerType = "DB")
        {
            if (loggerType.Equals("DB"))
            {
                string loggerKey = string.Format("{0}.Logging", LogType);
                if (IoC.IoCFactory.Container.IsRegistered<ILogger>(loggerKey))
                    return IoC.IoCFactory.Container.Resolve<ILogger>(loggerKey);
                loggerKey = "General.Logging";
                if (IoC.IoCFactory.Container.IsRegistered<ILogger>(loggerKey))
                    return IoC.IoCFactory.Container.Resolve<ILogger>(loggerKey);
                return IoC.IoCFactory.Container.Resolve<ILogger>();
            }
            else if (loggerType.Equals("FILE"))
            {
                return FileLogger.GetInstance();
            }
            else { return null; }
        }

        private static LogEntry refineLogEntry(LogEntry logEntry)
        {
            logEntry.Environemt = string.Join(", ", logEntry.Environemt, string.Format("Server OS={0}, Server .NET={1}", Environment.OSVersion, Environment.Version));
            if (AppConfiguration.HttpContext != null && AppConfiguration.HttpContext.Request != null)
            {
                logEntry.Environemt = string.Join(", ", logEntry.Environemt, string.Format("UserAgent={0}, HttpMethod={1}, IsSecureConnection={2}, UserHostAddress={3} ({4})",
                                                                            AppConfiguration.HttpContext.Request.UserAgent,
                                                                            AppConfiguration.HttpContext.Request.HttpMethod,
                                                                            AppConfiguration.HttpContext.Request.IsSecureConnection,
                                                                            AppConfiguration.HttpContext.Request.UserHostAddress,
                                                                            (AppConfiguration.HttpContext.Request.IsLocal ? "Local Request" : "Remote Request")
                                                                            )
                                                 );
            }
            logEntry.Environemt = logEntry.Environemt.TrimStart(", ".ToCharArray());

            if (logEntry.ExtraInfo != null && AppConfiguration.HttpContext != null)
                logEntry.ExtraInfo = string.Join(", ", logEntry.ExtraInfo, string.Format("SessionID={0}", AppConfiguration.HttpContext.Session.SessionID))
                                       .TrimStart(", ".ToCharArray());
            return logEntry;
        }

        private static LogEntry prepareLogEntry(LogEntry logEntry)
        {
            logEntry.UTCDate = AppConfiguration.UTCDateTime;
            logEntry.CultureId = AppConfiguration.Culture.Name;
            string tempUser = string.Empty;
            logEntry.UserId = Constants.AnonymousUser;
            if (AppConfiguration.TryGetCurrentUser(ref tempUser))
                logEntry.UserId = tempUser;
            logEntry.RequestURL = AppConfiguration.CurrentRequestURL.ToString();

            // caller method information. indicates where the custom logging function were called
            var frameIndex = 2;
            StackFrame frame = new StackFrame(frameIndex);
            while (frame != null && frame.GetMethod().DeclaringType.Assembly.GetName().Name.StartsWith("Vaiona", StringComparison.CurrentCultureIgnoreCase))
            {
                frameIndex++;
                frame = new StackFrame(frameIndex);
            }
            logEntry.AssemblyName = frame.GetMethod().DeclaringType.Assembly.GetName().Name;//
            logEntry.AssemblyVersion = frame.GetMethod().DeclaringType.Assembly.GetName().Version.ToString();
            logEntry.ClassName = frame.GetMethod().DeclaringType.FullName;
            logEntry.MethodName = frame.GetMethod().Name.TrimStart("<".ToCharArray()).TrimEnd(">".ToCharArray()); // not obvious why the method name is wrapped in <>??

            return logEntry;
        }

        private delegate void CustomLogDelegate(CustomLogEntry logEntry);

        public static void LogCustom(CustomLogEntry logEntry)
        {
            //if (!AppConfiguration.IsLoggingEnable || !AppConfiguration.IsCustomLoggingEnable)
            //    return;
            ILogger logger = create(logEntry.LogType);
            logEntry = (CustomLogEntry)prepareLogEntry(logEntry);
            logEntry = (CustomLogEntry)refineLogEntry(logEntry);
            CustomLogDelegate dlgt = new CustomLogDelegate(logger.LogCustom);
            IAsyncResult ar = dlgt.BeginInvoke(logEntry, null, null);
        }

        public static void LogCustom(string message)
        {
            //if (!AppConfiguration.IsLoggingEnable || !AppConfiguration.IsCustomLoggingEnable)
            //    return;
            CustomLogEntry logEntry = new CustomLogEntry();
            logEntry.LogType = LogType.Custom;
            logEntry.Desription = message;

            LogCustom(logEntry);
        }

        private delegate void MethodLogDelegate(MethodLogEntry logEntry);

        public static void LogMethod(MethodLogEntry logEntry)
        {
            ILogger logger = create(logEntry.LogType);
            logEntry = (MethodLogEntry)refineLogEntry(logEntry);
            MethodLogDelegate dlgt = new MethodLogDelegate(logger.LogMethod);
            IAsyncResult ar = dlgt.BeginInvoke(logEntry, null, null);
        }

        private delegate void DataLogDelegate(DataLogEntry logEntry);

        public static void LogData(DataLogEntry logEntry) // subject to remove
        {
            ILogger logger = create(logEntry.LogType);
            logEntry = (DataLogEntry)prepareLogEntry(logEntry);
            logEntry = (DataLogEntry)refineLogEntry(logEntry);
            logEntry.Desription = string.Format("Entity {0} of type '{1}' is in the '{2}' state.", logEntry.ObjectId, logEntry.ObjectType, logEntry.State);
            DataLogDelegate dlgt = new DataLogDelegate(logger.LogData);
            IAsyncResult ar = dlgt.BeginInvoke(logEntry, null, null);
        }

        public static void LogData(string objectId, string objectType, CrudState state)
        {
            DataLogEntry logEntry = new DataLogEntry()
            {
                LogType = LogType.Data,
                ObjectId = objectId,
                ObjectType = objectType,
                State = state,
            };
            LogData(logEntry);
        }

        private delegate void RelationLogDelegate(RelationLogEntry logEntry);

        public static void LogDataRelation(RelationLogEntry logEntry)
        {
            ILogger logger = create(logEntry.LogType);
            logEntry = (RelationLogEntry)prepareLogEntry(logEntry);
            logEntry = (RelationLogEntry)refineLogEntry(logEntry);
            logEntry.Desription = string.Format("A relationship between entity {0} of type '{1}' and entity {2} of type '{3}' is {4}.",
            logEntry.SourceObjectId, logEntry.SourceObjectType, logEntry.DestinationObjectId, logEntry.DestinationObjectType, logEntry.State);
            RelationLogDelegate dlgt = new RelationLogDelegate(logger.LogRelation);
            IAsyncResult ar = dlgt.BeginInvoke(logEntry, null, null);
            logger.LogRelation(logEntry);
        }

        public static void LogDataRelation(string sourceObjectId, string sourceObjectType, string destinationObjectId, string destinationObjectType, CrudState state)
        {
            RelationLogEntry logEntry = new RelationLogEntry()
            {
                LogType = LogType.Relation,
                SourceObjectId = sourceObjectId,
                SourceObjectType = sourceObjectType,
                DestinationObjectId = destinationObjectId,
                DestinationObjectType = destinationObjectType,
                State = state,
            };
            LogDataRelation(logEntry);
        }
    }
}