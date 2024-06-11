using PostSharp.Aspects;
using System;
using System.Diagnostics;
using Vaiona.Entities.Logging;
using Vaiona.Utils.Cfg;

namespace Vaiona.Logging.Aspects
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class MeasurePerformanceAttribute : OnMethodBoundaryAspect
    {
        public override sealed void OnEntry(MethodExecutionArgs args)
        {
            if (!AppConfiguration.IsLoggingEnable || !AppConfiguration.IsPerformanceLoggingEnable)
                return;

            var sw = new Stopwatch();
            sw.Start();
            args.MethodExecutionTag = sw;
        }

        public override sealed void OnExit(MethodExecutionArgs args)
        {
            if (!AppConfiguration.IsLoggingEnable || !AppConfiguration.IsPerformanceLoggingEnable)
                return;
            if (args.Exception != null) // don't log if there is am exception in the call context. it is likely captured by an exception logger
                return;

            var sw = (Stopwatch)args.MethodExecutionTag;
            sw.Stop();

            MethodLogEntry mLog = new MethodLogEntry();

            mLog.UTCDate = AppConfiguration.UTCDateTime;
            mLog.CultureId = AppConfiguration.Culture.Name;
            string tempUser = string.Empty;
            mLog.UserId = Constants.AnonymousUser;
            if (AppConfiguration.TryGetCurrentUser(ref tempUser))
                mLog.UserId = tempUser;
            mLog.RequestURL = AppConfiguration.CurrentRequestURL.ToString();

            mLog.AssemblyName = args.Method.DeclaringType.Assembly.GetName().Name;//
            mLog.AssemblyVersion = args.Method.DeclaringType.Assembly.GetName().Version.ToString();//
            mLog.ClassName = args.Method.DeclaringType.FullName;
            mLog.MethodName = args.Method.Name.TrimStart("~".ToCharArray());

            mLog.ProcessingTime = sw.ElapsedMilliseconds;
            mLog.Desription = "NA";

            mLog.LogType = LogType.Performance;
            LoggerFactory.LogMethod(mLog);
#if DEBUG
            Debug.WriteLine(string.Format("Diagnose is called on {0}.{1} at {2}", mLog.ClassName, mLog.MethodName, mLog.UTCDate));
#endif
        }
    }
}