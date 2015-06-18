using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BExIS.Ddm.Api;
using BExIS.Ddm.Model;
using BExIS.Ddm.Providers.LuceneProvider;
using BExIS.Web.Shell.Areas.DDM.Models;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DDM.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /DDM/DataDiscoveryManager/

        public ActionResult Index()
        {
            return View();
        }

        #region SearchDesigner


        public ActionResult SearchDesigner()
        {
            try
            {
                //if (Session["searchAttributeList"] == null)
                //{
                ISearchDesigner sd = GetSearchDesigner();

                    Session["searchAttributeList"] = GetListOfSearchAttributeViewModels(sd.Get());
                    Session["metadatNodes"] = sd.GetMetadataNodes();
                    ViewData["windowVisible"] = false;
                    Session["IncludePrimaryData"] = sd.IsPrimaryDataIncluded();
                //}

            }
            catch(Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
            }

            return View((List<SearchAttributeViewModel>)Session["searchAttributeList"]);
        }

        [GridAction]
        public ActionResult _CustomSearchDesignerGridBinding(GridCommand command)
        {
            try{

                if (Session["searchAttributeList"] == null)
                {
                    ISearchDesigner sd = GetSearchDesigner();

                    Session["searchAttributeList"] = GetListOfSearchAttributeViewModels(sd.Get());
                    Session["metadatNodes"] = sd.GetMetadataNodes();
                    ViewData["windowVisible"] = false;
                }

                return View("SearchDesigner", new GridModel((List<SearchAttributeViewModel>)Session["searchAttributeList"]));
            }
            catch(Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
                return View();
            }
        }

        #region Search Attribute
            
            public ActionResult Add()
            {
                List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];

                SearchAttributeViewModel sa = new SearchAttributeViewModel();
                sa.id = searchAttributeList.Count;

                ViewData["windowVisible"] = true;
                ViewData["selectedSearchAttribute"] = sa;
                return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
            }

            public ActionResult Edit(int id)
            {
                List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];

                ViewData["windowVisible"] = true;
                ViewData["selectedSearchAttribute"] = searchAttributeList.Where(p => p.id.Equals(id)).First();
                return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
                //return PartialView("_editSearchAttribute", searchAttributeList.Where(p => p.id.Equals(id)).First());
            }

            public ActionResult Delete(int id)
            {
                List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];
                searchAttributeList.Remove(searchAttributeList.Where(p => p.id.Equals(id)).First());
                Session["searchAttributeList"] = searchAttributeList;
                ViewData["windowVisible"] = false;

                //update config FileStream
                SaveConfig();

                return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
            }


            public ActionResult Save(SearchAttributeViewModel model)
            {
                if (ModelState.IsValid)
                {
                    //if (submit != null)
                    //{
                        List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];

                        if (searchAttributeList.Where(p => p.id.Equals(model.id)).Count() > 0)
                        {
                            SearchAttributeViewModel temp = searchAttributeList.Where(p => p.id.Equals(model.id)).First();
                            searchAttributeList[searchAttributeList.IndexOf(temp)] = model;
                        }
                        else
                        {
                            searchAttributeList.Add(model);
                        }

                        ISearchDesigner sd = GetSearchDesigner();

                        //sd.Set(searchAttributeList);

                        Session["searchAttributeList"] = searchAttributeList;
                        ViewData["windowVisible"] = false;

                        //create new config FileStream
                        SaveConfig();
                    //}

                    return Json(true);
                }
                else
                {
                    ViewData["windowVisible"] = true;
                }

                return Json(false);
                //return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
            }

        #endregion

        public ActionResult ChangeIncludePrimaryData(bool includePrimaryData)
        {
            Session["IncludePrimaryData"] = includePrimaryData;
            SaveConfig();

            return Content("true");
        }

        public ActionResult CloseWindow()
        {

            ViewData["windowVisible"] = false;

            return Content("");
        }


        public void SaveConfig()
        {

            if (Session["searchAttributeList"] != null)
            {
                List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];
                ISearchDesigner sd = GetSearchDesigner();

                try
                {
                    sd.Set(GetListOfSearchAttributes(searchAttributeList), (bool)Session["IncludePrimaryData"]);
                    Session["searchAttributeList"] = searchAttributeList;
                    ViewData["windowVisible"] = false;
                }
                catch(Exception e)
                {

                }

                //sd.Reload();
                //searchConfigFileInUse = false;
            }

            //return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
        }

        public ActionResult ResetConfig()
        {
            try
            {
                ISearchDesigner sd = GetSearchDesigner();
                sd.Reset();
                Session["searchAttributeList"] = GetListOfSearchAttributeViewModels(sd.Get());
                Session["metadatNodes"] = sd.GetMetadataNodes();
                ViewData["windowVisible"] = false;
            }
            catch(Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
            }

            return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
        }

        public ActionResult ReloadConfig()
        {
            ISearchDesigner sd = GetSearchDesigner();
            sd.Reload();

            //ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

            //((SearchProvider)provider).RefreshIndex();

            return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
        }
        
        private List<SearchAttribute> GetListOfSearchAttributes(List<SearchAttributeViewModel> listOfViewModels)
        {
            List<SearchAttribute> listOfSearchAttributes = new List<SearchAttribute>();

            foreach (SearchAttributeViewModel savm in listOfViewModels)
            {
                listOfSearchAttributes.Add(SearchAttributeViewModel.GetSearchAttribute(savm));
            }

            return listOfSearchAttributes;
        }

        private List<SearchAttributeViewModel> GetListOfSearchAttributeViewModels(List<SearchAttribute> listOfSearchAttributes)
        {
            List<SearchAttributeViewModel> listOfSearchAttributeViewModels = new List<SearchAttributeViewModel>();

            foreach (SearchAttribute sa in listOfSearchAttributes)
            {
                listOfSearchAttributeViewModels.Add(SearchAttributeViewModel.GetSearchAttributeViewModel(sa));
            }

            return listOfSearchAttributeViewModels;
        }

        #region Validation

        [HttpGet]
        public JsonResult ValidateSourceName(string sourceName, long id)
        {
            List<SearchAttributeViewModel> list = (List<SearchAttributeViewModel>)Session["searchAttributeList"];

            if (list != null)
            {
                foreach(SearchAttributeViewModel sa in list )
                {
                    if (sa.sourceName.ToLower().Equals(sourceName.ToLower()) && sa.id != id)
                    {
                        string error = String.Format(CultureInfo.InvariantCulture, "Source name already exists.", sourceName);

                        return Json(error, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(true, JsonRequestBehavior.AllowGet);

            }
            else
            {
                string error = String.Format(CultureInfo.InvariantCulture, "Is not possible to compare Sourcename with a empty list of search attributes.", sourceName);

                return Json(error, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #endregion

        #region ReIndex

        public ActionResult RefreshSearch()
        {
            ISearchDesigner sd = GetSearchDesigner();

            bool success = false;

            try
            {
                sd.Reload();
                success = true;
            }
            catch (Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
                success = false;
            }
            finally
            {
                sd.Dispose();
            }

            if(success)
                    return RedirectToAction("Index","Home",new RouteValueDictionary{{ "area", "DDM" }});
            else
                    return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);

        }

        #endregion

        #region

        private SearchDesigner GetSearchDesigner()
        { 

            if (Session["SearchDesigner"] != null)
            {
                return (SearchDesigner)Session["SearchDesigner"];
            }

            return new SearchDesigner();
        }

        private void SetSearchDesigner(SearchDesigner searchDesigner)
        {
            Session["SearchDesigner"] = searchDesigner;
        }

        public ActionResult AddMetadataNode()
        {
            return PartialView("_metadataNode", "");
        }

        #endregion


    }
}
