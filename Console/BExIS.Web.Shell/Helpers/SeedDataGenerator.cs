using BExIS.Security.Services.Subjects;

namespace BExIS.Web.Shell.Helpers
{
    public class SeedDataGenerator
    {
        public static void Init()
        {
            var groupManager = new GroupManager();

            // System Groups
            if (!groupManager.Exists("everyone", true))
                groupManager.Create("everyone", "This group represents all registered users.", true);

            if (!groupManager.Exists("anonymous", true))
                groupManager.Create("anonymous", "This group represents all users (incl. both, registered and non-registered users).", true);
        }
    }
}