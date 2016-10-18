using System.Web.Mvc;
using System.Xml.Linq;

namespace BExIS.Web.Shell.Areas.DIM
{
    public class DIMAreaRegistration:AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "DIM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "DIM_default",
                "DIM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }

        public XElement RegisterMenus(AreaRegistrationContext context)
        {
            XElement moduleNode = new XElement(this.AreaName);
            XElement export = new XElement("Export");
                export.SetAttributeValue("Parent", "Settings");
                export.SetAttributeValue("Label", "Export Metadata");
                export.SetAttributeValue("Area", "dim");
                export.SetAttributeValue("Controller", "Admin");
                export.SetAttributeValue("Action", "Index");
            moduleNode.Add(export);
            return moduleNode;
        }

    }
}