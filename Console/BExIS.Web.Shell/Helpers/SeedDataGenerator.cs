using BExIS.Security.Services.Objects;

namespace BExIS.Web.Shell.Helpers
{
    public class SeedDataGenerator
    {
        public static void Init()
        {
            var operationManager = new OperationManager();

            operationManager.Create("Shell", "Account", "*");
            operationManager.Create("Shell", "Home", "*");
        }
    }
}