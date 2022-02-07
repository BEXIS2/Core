using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using System.Configuration;
using Vaiona.IoC;
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

                GeneralSettings generalSettings = IoCFactory.Container.Resolve<GeneralSettings>();

                es.Send(subject,
                    result,
                    generalSettings.SystemEmail
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