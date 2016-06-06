using System.Web.Mvc;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.Sam
{
    public class SAMAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SAM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "SAM_default",
                "SAM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );

            if (AppConfiguration.CreateDatabase) SAM.Helpers.SamSeedDataGenerator.GenerateSeedData();
        }

        public XElement RegisterMenus(AreaRegistrationContext context)
        {
            XElement moduleNode = new XElement(this.AreaName);
            XElement myAccount = new XElement("MyAccount");
                myAccount.SetAttributeValue("Parent", "User");
                myAccount.SetAttributeValue("Label", "My Account");
                myAccount.SetAttributeValue("Area", "sam");
                myAccount.SetAttributeValue("Controller", "MyAccount");
                myAccount.SetAttributeValue("Action", "Account");
            moduleNode.Add(myAccount);
            return moduleNode;
        }
    }
}