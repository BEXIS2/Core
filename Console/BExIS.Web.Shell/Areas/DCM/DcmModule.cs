using BExIS.Modules.Dcm.UI.Helpers;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI
{
    public class DcmModule : ModuleBase
    {
        public DcmModule() : base("dcm")
        {
        }

        public override void Install()
        {
            base.Install();
            DcmSeedDataGenerator.GenerateSeedData();
        }
    }
}
