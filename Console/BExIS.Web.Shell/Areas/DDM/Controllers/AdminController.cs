using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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
                    ISearchDesigner sd = new SearchDesigner();

                    Session["searchAttributeList"] = GetListOfSearchAttributeViewModels(sd.Get());
                    Session["metadatNodes"] = sd.GetMetadataNodes();
                    ViewData["windowVisible"] = false;
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
                    ISearchDesigner sd = new SearchDesigner();

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

                return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
            }

            public ActionResult Save(string submit, SearchAttributeViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (submit != null)
                {
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

                    ISearchDesigner sd = new SearchDesigner();

                    //sd.Set(searchAttributeList);

                    Session["searchAttributeList"] = searchAttributeList;
                    ViewData["windowVisible"] = false;
                }
            }
            else
            {
                ViewData["windowVisible"] = true;
            }


            return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
        }

        #endregion

        public ActionResult CloseWindow()
        {

            ViewData["windowVisible"] = false;

            return Content("");
        }


        public ActionResult SaveConfig()
        {

            if (Session["searchAttributeList"] != null)
            {
                List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];
                ISearchDesigner sd = new SearchDesigner();
                sd.Set(GetListOfSearchAttributes(searchAttributeList));
                Session["searchAttributeList"] = searchAttributeList;
                ViewData["windowVisible"] = false;

                //sd.Reload();

                //searchConfigFileInUse = false;
            }

            return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
        }

        public ActionResult ResetConfig()
        {
            try
            {
                ISearchDesigner sd = new SearchDesigner();
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
            ISearchDesigner sd = new SearchDesigner();
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

                public JsonResult ValidateSourceName(string sourceName)
                {
                    List<SearchAttributeViewModel> list = (List<SearchAttributeViewModel>)Session["searchAttributeList"];

                    if (list != null)
                    {
                        if (list.Where(a => a.sourceName.Equals(sourceName)).Count() > 0)
                        {
                            string error = String.Format(CultureInfo.InvariantCulture, "Source name already exists.", sourceName);

                            return Json(error, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(true, JsonRequestBehavior.AllowGet);
                        }
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

        public ActionResult ReIndexSearch()
        {
            return View();
        }

        public ActionResult RefreshSearch()
        {
            try
            {
                ISearchDesigner sd = new SearchDesigner();
                sd.Reload();
            }
            catch (Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);
            }

            return View("ReIndexSearch");
        }


      

        #endregion


    }
}
