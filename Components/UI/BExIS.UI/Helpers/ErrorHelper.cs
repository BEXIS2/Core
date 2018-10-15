using BExIS.Security.Services.Utilities;
using System.Configuration;
using Vaiona.Logging;

namespace BExIS.UI.Helpers
{
    public class ErrorHelper
    {
        public static void SendEmailWithErrors(string result)
        {
            try
            {

                var es = new EmailService();
                var subject = "Error in system";
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
