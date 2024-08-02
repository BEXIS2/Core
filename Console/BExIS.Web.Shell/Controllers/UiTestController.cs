namespace BExIS.Web.Shell.Controllers
{
    //public class UiTestController : Controller
    //{
    //    public ActionResult Index()
    //    {
    //        return View();
    //    }

    //    #region TreeView

    //    [HttpGet]
    //    public JsonResult TreeviewData()
    //    {
    //        Treeview treeview = new Treeview();

    //        treeview.showcheckbox = true;
    //        treeview.showcount = true;
    //        treeview.target = "_self";

    //        TreeviewItem a = new TreeviewItem()
    //        {
    //            label = "A",
    //            descripton = "description of a",
    //            count = 100,
    //            value = "1"
    //        };

    //        TreeviewItem a_child1 = new TreeviewItem()
    //        {
    //            label = "A_child1",
    //            descripton = "description of a child 1",
    //            count = 10,
    //            value = "11",
    //        };

    //        a.items.Add(a_child1);

    //        TreeviewItem a_child2 = new TreeviewItem()
    //        {
    //            label = "A_child2",
    //            descripton = "description of a child 2",
    //            count = 120,
    //            value = "12"
    //        };

    //        a.items.Add(a_child2);

    //        treeview.data.Add(a);

    //        treeview.data.Add(new TreeviewItem()
    //        {
    //            label = "B",
    //            descripton = "description of B",
    //            count = 1020,
    //            value = "2"
    //        });

    //        return Json(treeview, JsonRequestBehavior.AllowGet);
    //    }

    //    public JsonResult Select(long id)
    //    {
    //        return Json(true, JsonRequestBehavior.AllowGet);
    //    }

    //    #endregion

    //    #region datatables
    //    [HttpPost]
    //    public ActionResult Load(DataTableRecieverModel model)
    //    {
    //        using (UserManager userManager = new UserManager())
    //        {
    //            try
    //            {
    //                int skip = model.Start;

    //                IQueryable<User> data = null;

    //                data = userManager.Users.AsQueryable();

    //                //search
    //                if (!string.IsNullOrEmpty(model.Search.Value))
    //                {
    //                    string searchValue = model.Search.Value.ToLower();
    //                    data = data.Where(i =>
    //                        (i.Name != null && i.Name.ToLower().Contains(searchValue)) ||
    //                        (i.DisplayName != null && i.DisplayName.ToLower().Contains(searchValue)) ||
    //                        (i.Email != null && i.Email.ToLower().Contains(searchValue)));
    //                }

    //                int filteredRows = data.ToList().Count();

    //                //order by
    //                var sorting = string.Join(",", model.Order.Select(o => model.Columns[o.Column].Data + " " + o.Dir));
    //                if (!string.IsNullOrEmpty(sorting))
    //                {
    //                    data = data.OrderBy(sorting);
    //                }

    //                //paging
    //                data = data.Skip(skip).Take(model.Length);

    //                int countAll = userManager.Users.Count();

    //                List<UserModel> tmp = new List<UserModel>();
    //                data.ToList().ForEach(u => tmp.Add(new UserModel(u)));

    //                DataTableSendModel sendModel = new DataTableSendModel();
    //                sendModel.data = tmp;
    //                sendModel.draw = model.Draw;
    //                sendModel.recordsTotal = countAll;
    //                sendModel.recordsFiltered = filteredRows;

    //                return Json(sendModel);
    //            }
    //            catch (Exception exception)
    //            {
    //                string json = "{\"error\":\"" + exception.Message + "\"}";

    //                return Json(json);
    //            }
    //        }
    //    }

    //    #endregion

    //}

    //public class UserModel
    //{
    //    public long Id { get; set; }
    //    public string Name { get; set; }
    //    public string FullName { get; set; }
    //    public string Email { get; set; }

    //    public UserModel(User user)
    //    {
    //        Id = user.Id;
    //        Name = user.Name;
    //        Email = user.Email;
    //    }
    //}
}