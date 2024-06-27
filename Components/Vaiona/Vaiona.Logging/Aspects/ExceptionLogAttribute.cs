using PostSharp.Aspects;
using System;
using System.Diagnostics;
using Vaiona.Entities.Logging;
using Vaiona.Utils.Cfg;

namespace Vaiona.Logging.Aspects
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class LogExceptionsAttribute : OnExceptionAspect
    {
        public override sealed void OnException(MethodExecutionArgs args)
        {
            //base.OnException(eventArgs);

            if (!AppConfiguration.IsLoggingEnable || !AppConfiguration.IsExceptionLoggingEnable)
                return;

            MethodLogEntry mLog = new MethodLogEntry();

            mLog.UTCDate = AppConfiguration.UTCDateTime;
            mLog.CultureId = AppConfiguration.Culture.Name;
            string tempUser = string.Empty;
            mLog.UserId = Constants.AnonymousUser;
            if (AppConfiguration.TryGetCurrentUser(ref tempUser))
                mLog.UserId = tempUser;
            mLog.RequestURL = AppConfiguration.CurrentRequestURL.ToString();

            mLog.AssemblyName = args.Method.DeclaringType.Assembly.GetName().Name;//
            mLog.AssemblyVersion = args.Method.DeclaringType.Assembly.GetName().Version.ToString();
            mLog.ClassName = args.Method.DeclaringType.FullName;
            mLog.MethodName = args.Method.Name.TrimStart("~".ToCharArray());

            mLog.Desription = args.Exception.Message; // recursive message traversal would be an improvement!
            mLog.ExtraInfo = string.Format("ExceptionClass={0}", args.Exception.GetType().Name);

            mLog.LogType = LogType.Exception;
            LoggerFactory.LogMethod(mLog);
#if DEBUG
            Debug.WriteLine(string.Format("Diagnose is called on {0}.{1} at {2}", mLog.ClassName, mLog.MethodName, mLog.UTCDate));
#endif
        }
    }
}