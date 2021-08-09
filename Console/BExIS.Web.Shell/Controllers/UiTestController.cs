using BExIS.UI.Models;
using System.Web.Mvc;


namespace BExIS.Web.Shell.Controllers
{
    public class UiTestController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult TreeviewData()
        {
            Treeview treeview = new Treeview();

            treeview.showcheckbox = true;
            treeview.showcount = true;
            treeview.target = "_self";

            TreeviewItem a = new TreeviewItem()
            {
                label = "A",
                descripton = "description of a",
                count = 100,
                value = "1"
            };

            TreeviewItem a_child1 = new TreeviewItem()
            {
                label = "A_child1",
                descripton = "description of a child 1",
                count = 10,
                value = "11",
            };

            a.items.Add(a_child1);

            TreeviewItem a_child2 = new TreeviewItem()
            {
                label = "A_child2",
                descripton = "description of a child 2",
                count = 120,
                value = "12"
            };

            a.items.Add(a_child2);

            treeview.data.Add(a);

            treeview.data.Add(new TreeviewItem()
            {
                label = "B",
                descripton = "description of B",
                count = 1020,
                value = "2"
            });


            return Json(treeview, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Select(long id)
        {

            return Json(true, JsonRequestBehavior.AllowGet);
        }
        
    }
}
