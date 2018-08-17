using BExIS.Security.Services.Utilities;
using System.Configuration;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;

namespace BExIS.UI.Helpers
{
    public class ErrorHelper
    {
        public static void SendEmailWithErrors(string result)
        {
            try
            {
                string AppId = "";
                if (!string.IsNullOrEmpty(AppConfiguration.ApplicationName) && !string.IsNullOrEmpty(AppConfiguration.ApplicationVersion))
                {
                    AppId = AppConfiguration.ApplicationName + " (" + AppConfiguration.ApplicationVersion + ") - ";
                }

                var es = new EmailService();
                var subject = AppId + "Error in system";
                es.Send(subject,
                    result,
                    ConfigurationManager.AppSettings["SystemEmail"]
                    );
            }
            catch (System.Web.HttpException ehttp)
            {
                // Write o the event log.
            }
        }

        public static void Log(string result)
        {
            LoggerFactory.GetFileLogger().LogCustom(result);
        }
    }
}
