using PostSharp.Aspects;
using System;
using System.Diagnostics;
using Vaiona.Entities.Logging;
using Vaiona.Utils.Cfg;

namespace Vaiona.Logging.Aspects
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [LinesOfCodeAvoided(1)]
    public class RecordCallAttribute : MethodInterceptionAspect
    {
        public override sealed void OnInvoke(MethodInterceptionArgs args)

        {
            if (!AppConfiguration.IsLoggingEnable || !AppConfiguration.IsCallLoggingEnable)
                return;
            base.OnInvoke(args);
            /// if invocation of the original method encounters any exception,
            /// the remaining code will not execute, hence no call is recorded.
            /// if needed the method should be tagged with an ExceptionLogAttribute, too.

            MethodLogEntry mLog = new MethodLogEntry();

            mLog.UTCDate = AppConfiguration.UTCDateTime;
            mLog.CultureId = AppConfiguration.Culture.Name;
            string tempUser = string.Empty;
            mLog.UserId = Constants.AnonymousUser;
            if (AppConfiguration.TryGetCurrentUser(ref tempUser))
                mLog.UserId = tempUser;
            mLog.RequestURL = AppConfiguration.CurrentRequestURL.ToString();
            //mLog = (MethodLogEntry)LoggerFactory.GetEnvironemntLogEntry();

            mLog.AssemblyName = args.Method.DeclaringType.Assembly.GetName().Name;//
            mLog.AssemblyVersion = args.Method.DeclaringType.Assembly.GetName().Version.ToString();
            mLog.ClassName = args.Method.DeclaringType.FullName;
            mLog.MethodName = args.Method.Name.TrimStart("~".ToCharArray()); // PostSharp renames the original method name be adding a leading tilde "~"

            mLog.LogType = LogType.Call;
            LoggerFactory.LogMethod(mLog);
#if DEBUG
            Debug.WriteLine(string.Format("Trace is called on {0}.{1} at {2}", mLog.ClassName, mLog.MethodName, mLog.UTCDate));
#endif
        }
    }
}