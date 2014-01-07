using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Search.Api;
using BExIS.Search.Model;
using BExIS.Search.Providers.LuceneProvider;
using BExIS.Web.Shell.Areas.Search.Helpers;
using BExIS.Web.Shell.Areas.Search.Models;
using Telerik.Web.Mvc;
using Vaiona.IoC;
using Vaiona.Util.Cfg;

namespace BExIS.Web.Shell.Areas.Search.Controllers
{
    public class HomeController : Controller
    {
        public bool searchConfigFileInUse = false;

        //ISearchProvider provider = null;
        //public HomeController()
        //{
        //    if (HttpContext.Session != null && HttpContext.Session["ISearchProvider"] != null)
        //        provider = HttpContext.Session["ISearchProvider"] as ISearchProvider;
        //    if (provider == null) // this check should be done by the IoC lifetime manager. Javad, 29.11.12
        //    {
        //        provider = IoCFactory.Container.Resolve<ISearchProvider>() as ISearchProvider;
        //        HttpContext.Session["ISearchProvider"] = provider;
        //    }
        //}

        /// <summary>
        /// is called when the Search View is selected
        /// 
        /// </summary>
        /// <param name="model">from type SearchDataModel</param>
        /// <returns></returns>
        //[Authorize(Roles="Guest")]
        public ActionResult Index()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

            if (provider.WorkingSearchModel.CriteriaComponent.SearchCriteriaList.Count > 0)
            {
                provider.WorkingSearchModel.CriteriaComponent.Clear();
                provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);
            }
            //var pp = IoCFactory.Container.ResolveAll<ISearchProvider>();

            SetSessionsToDefault();

            return View(provider);
        }

        /// <summary>
        /// This action is called when the search button is pressed
        /// </summary>
        /// <param name="autoComplete">search input as string</param>
        /// <param name="FilterList">selected filter</param>
        /// <param name="searchType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(string autoComplete, string FilterList, string searchType)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;


            if (searchType == "new")
            {
                Session["FilterAC"] = null;
                Session["SelectedIndexFilterAC"] = 0;
                Session["PropertiesDictionary"] = null;
               
                provider.WorkingSearchModel.CriteriaComponent.Clear();
            }

            SetSearchType(searchType);
            
            if (!provider.WorkingSearchModel.CriteriaComponent.ContainsSearchCriterion(FilterList, autoComplete, SearchComponentBaseType.Category))
            {
                provider.WorkingSearchModel.UpdateSearchCriteria(FilterList, autoComplete, SearchComponentBaseType.Category);
            }

            provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

            //reset searchType
            // after every search - searchType must be based on
            SetSearchType("basedon");

            return View(provider);
        }
        
        #region SearchHeader

        /// <summary>
        /// Is called when the user select a other filter
        /// </summary>
        /// <param name="SelectedFilter">selected filter</param>
        /// <param name="searchType"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult FilterByDropDownList(string SelectedFilter, string searchType)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            SetFilterAC(SelectedFilter);
            
            return View("Index", provider);
        }

        /// <summary>
        /// Is called when the user write a letter in Autocomplete User Component
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult _AutoCompleteAjaxLoading(string text)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            return new JsonResult { Data = new SelectList(provider.GetTextBoxSearchValues(text, GetFilterAC(), Session["SearchType"].ToString(), 10).SearchComponent.TextBoxSearchValues, "Value", "Name") };
        }

        /// <summary>
        /// Is called when the user change the SearchType
        /// </summary>
        /// <param name="value">consist the searchType</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeSearchValuesACBySearchType(string value)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            Session["SearchType"] = value;
            return View(provider.WorkingSearchModel);
        }

        #endregion

        
        #region Treeview - _searchFacets

        //+++++++++++++++++++++ TreeView onChecked Action +++++++++++++++++++++++++++
        /// <summary>
        /// Is called when a user select a facet in the treeview by using the checkox
        /// </summary>
        /// <param name="SelectedItem"> name of the selected item</param>
        /// <param name="Parent">name of the parent from the selected item</param>
        /// <param name="IsChecked">show the status of the checkbox (true = selected/false=deselected)</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CheckedTreeViewItem(string SelectedItem, string Parent)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            provider.WorkingSearchModel.UpdateSearchCriteria(Parent, SelectedItem, SearchComponentBaseType.Facet, true);
            provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

            return PartialView("_searchFacets", Tuple.Create(provider.WorkingSearchModel,provider.DefaultSearchModel.SearchComponent.Facets));
        }

        public ActionResult UpdateFacets()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            return PartialView("_searchFacets", Tuple.Create(provider.UpdateFacets(provider.WorkingSearchModel.CriteriaComponent),provider.DefaultSearchModel.SearchComponent.Facets));
        }

        public ActionResult GetDataForBreadCrumbView()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

            return PartialView("_searchBreadCrumb", provider.WorkingSearchModel);
        }

        //+++++++++++++++++++++ TreeView onSelect Action +++++++++++++++++++++++++++
        /// <summary>
        /// Is called when a user select a category in the treeview by select the item
        /// </summary>
        /// <param name="SelectedItem"> name of the selected item</param>
        /// <param name="Parent">name of the parent from the selected item</param>
        /// <param name="IsChecked">show the status of the checkbox (true = selected/false=deselected)</param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult OnSelectTreeViewItem(string SelectedItem, string Parent)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            provider.WorkingSearchModel.UpdateSearchCriteria(Parent, SelectedItem, SearchComponentBaseType.Facet, true);
            provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

            return View("Index", provider);
        }

        /// <summary>
        /// Add a selected Facets to the Search Values
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddFacetsToSearch()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

            List<string> selectedValues = new List<string>();
            string parent = GetParentOfSelectAbleCategories();

            //data.ToList().ForEach(p => selectedValues.Add(p.ToString()));

            for (int i = 0; i < this.Request.Form.AllKeys.Count() - 1; i++)
            {
                int index = Convert.ToInt32(this.Request.Form.AllKeys[i]);
                string[] temp = this.Request.Form.GetValues(i);
                string value = temp[0];

                if (value == "true" || value == "True")
                {
                    selectedValues.Add(GetSelectAbleCategoryList().ElementAt(index).Name);
                }
            }

            provider.WorkingSearchModel.UpdateSearchCriteria(parent, selectedValues, SearchComponentBaseType.Facet, true);
            provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

            return View("Index",provider);

        }

        /// <summary>
        /// When the user click on the more button in the treeview 
        /// a window pops up an show all categories from the main categorie
        /// </summary>
        /// <param name="parent">name of the parent where the more button is inside</param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult ShowMoreWindow(string parent)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

            SetParentOfSelectAbleCategories(parent);

            var facet = provider.DefaultSearchModel.SearchComponent.Facets.Where(p => p.Name.Equals(parent, StringComparison.InvariantCulture)).FirstOrDefault();
            SetParentOfSelectAbleCategories(parent);

           // List<Facet> sortedList = facet.Childrens.OrderBy(p => p.DisplayName).ToList();

            SetSelectAbleCategoryList(facet.Childrens.OrderBy(p => p.DisplayName).ToList());

            return PartialView("_windowCheckBoxList", provider.WorkingSearchModel);
        }

        #endregion
        
        #region breadcrumbView
        //+++++++++++++++++++++BreadCrumb Update Data +++++++++++++++++++++++++++

        /// <summary>
        /// Is called when the user click on the labels in the breadcrumb view
        /// 
        /// </summary>
        /// <param name="value">selected value</param>
        /// <param name="parent">patrent of selected value</param>
        /// <param name="model"></param>
        /// <returns></returns>
        public ActionResult OnClickBreadCrumbItem(string value, string parent)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

            RemoveFromPropertiesDic(parent, value, provider.WorkingSearchModel.CriteriaComponent);

            // if value exist / user clicked value
            if (value != null && parent != null)
            {
                provider.WorkingSearchModel.CriteriaComponent.RemoveValueOfSearchCriteria(parent, value);
            }

            provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

            return View("Index", provider);
        }

        #endregion
        
        #region Datagrid
        // +++++++++++++++++++++ DataGRID Action +++++++++++++++++++++++++++
        
        [GridAction]
        public ActionResult _CustomBinding(GridCommand command)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            DataTable table = provider.Get(provider.WorkingSearchModel.CriteriaComponent).ResultComponent.ConvertToDataTable();

            return View(new GridModel(table));
        }
        
        [GridAction]
        public ActionResult _CustomPrimaryDataBinding(GridCommand command, int datasetID)
        {
            DatasetManager dm = new DatasetManager();
            DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
            List<DataTuple> dsVersionTuples = dm.GetDatasetVersionEffectiveTuples(dsv);
            DataTable table = SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dsVersionTuples);

            return View(new GridModel(table));
        }

        [GridAction]
        public ActionResult _CustomDataStructureBinding(GridCommand command, int datasetID)
        {
            long id = (long)datasetID;
            DatasetManager dm = new DatasetManager();
            DatasetVersion ds = dm.GetDatasetLatestVersion(id);
            if (ds != null)
            {
                DataStructureManager dsm = new DataStructureManager();
                StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(ds.Dataset.DataStructure.Id);
                dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);
                //StructuredDataStructure sds = (StructuredDataStructure)(ds.Dataset.DataStructure.Self);
                DataTable table = SearchUIHelper.ConvertStructuredDataStructureToDataTable(sds);

                return View(new GridModel(table));
            }

            return View(new GridModel(new DataTable()));
        }

        public ActionResult ShowPreviewDataStructure(int datasetID)
        {
            DatasetManager dm = new DatasetManager();
            DatasetVersion ds = dm.GetDatasetLatestVersion((long)datasetID);
            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(ds.Dataset.DataStructure.Id);

            Tuple<StructuredDataStructure, int> m = new Tuple<StructuredDataStructure, int>(
                sds,
                datasetID
               );

            return PartialView("_previewDatastructure", m);
        }
        
        public ActionResult ShowMetaData(int datasetID)
        {
            DatasetManager dm = new DatasetManager();
            DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
            Session["Metadata"] = SearchUIHelper.ConvertXmlToHtml(dsv.Metadata.InnerXml.ToString(),"\\UI\\HtmlShowMetadata.xsl");

            return View();
        }

        //Javad: this method is called on the hover of the search results.
        public ActionResult ShowPrimaryData(int datasetID)
        {
            DatasetManager dm = new DatasetManager();
            DatasetVersion dsv = dm.GetDatasetLatestVersion(datasetID);
            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(dsv.Dataset.DataStructure.Id);
            List<DataTuple> dsVersionTuples = dm.GetDatasetVersionEffectiveTuples(dsv);
            string downloadUri = "";

            if(dsv.ContentDescriptors.Count>0)
            {
                if(dsv.ContentDescriptors.Count(p=>p.Name.Equals("generated")) == 1)
                {
                    downloadUri = dsv.ContentDescriptors.Where(p=>p.Name.Equals("generated")).First().URI;
                }

            }
            Tuple<DataTable, int, StructuredDataStructure, String> m = new Tuple<DataTable, int, StructuredDataStructure,String>(
               SearchUIHelper.ConvertPrimaryDataToDatatable(dsv, dsVersionTuples),
               datasetID,
               sds,
               downloadUri
               );
            
            return View(m);
        }

        public ActionResult DownloadPrimaryData(string path)
        {
            string[] temp = path.Split('\\');
            // define a correct name
            return File(Path.Combine(AppConfiguration.DataPath, path), "application/xlsm",temp.Last());
        }


        public ActionResult SetResultViewVar(string key, string value)
        {
            Session[key] = value;

            return this.Json(new { success = true });
        }

        #endregion

        #region Properties _searchProperties
        //+++++++++++++++++++++ Properties Sliders Action +++++++++++++++++++++++++++
        [HttpPost]
        public ActionResult FilterByRangeSlider(int start, int end, string parent)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            return PartialView("_searchBreadcrumb", provider.WorkingSearchModel);
        }

        [HttpPost]
        public ActionResult FilterBySlider(int value, string parent)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

            UpdatePropertiesDic(parent, value.ToString());
            provider.WorkingSearchModel.UpdateSearchCriteria(parent, value.ToString(), SearchComponentBaseType.Property, false, true);

            return PartialView("_searchBreadcrumb", provider.Get(provider.WorkingSearchModel.CriteriaComponent));
        }

        //+++++++++++++++++++++Properties DropDown Action +++++++++++++++++++++++++++
        [HttpPost]
        public ActionResult FilterByDropDown(string value, string node)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

            UpdatePropertiesDic(node, value);
            provider.WorkingSearchModel.UpdateSearchCriteria(node, value.ToString(), SearchComponentBaseType.Property);

            return PartialView("_searchBreadcrumb", provider.Get(provider.WorkingSearchModel.CriteriaComponent, 10, 1));
        }

        //+++++++++++++++++++++Properties RadioButton Action +++++++++++++++++++++++++++
        // currently radionbuttons on View searchProperties
        [HttpPost]
        public ActionResult FilterByCheckBox(string value, string node, bool isChecked)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            UpdatePropertiesDic(node, value);
            provider.WorkingSearchModel.UpdateSearchCriteria(node, value.ToString(), SearchComponentBaseType.Property);

            return PartialView("_searchBreadcrumb", provider.Get(provider.WorkingSearchModel.CriteriaComponent));
        }

        
        public void UpdatePropertiesDic(string name, string value)
        {
            if (name != null)
            {
                if (!PropertiesDic.ContainsKey(name))
                {
                    PropertiesDic.Add(name, "");
                }

                foreach (KeyValuePair<string, string> kvp in PropertiesDic)
                {
                    if (kvp.Key == name)
                    {
                        PropertiesDic.Remove(name);
                        PropertiesDic.Add(name, value);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Remove a property from the Dictionary
        /// example: 
        /// grassland: all | yes | no
        /// grassland is name
        /// value is (all | yes | no)
        /// </summary>
        /// <param name="name">Name of the property</param>
        /// <param name="value">Value of the property</param>
        public void RemoveFromPropertiesDic(string name, string value, SearchCriteria sc)
        {
            if (name != null)
            {
                if (sc.GetProperty(name) != null)
                {
                    string datasourceKey = sc.GetProperty(name).DataSourceKey;

                    if (PropertiesDic.ContainsKey(datasourceKey))
                    {
                        foreach (KeyValuePair<string, string> kvp in PropertiesDic)
                        {
                            if (kvp.Value == value && kvp.Key == datasourceKey)
                            {
                                PropertiesDic.Remove(datasourceKey);
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion
        
        #region Dictionary (Search/Properties)

        private Dictionary<string, string> PropertiesDic
        {
            get
            {
                object o = this.Session["PropertiesDictionary"];
                if (o == null)
                    this.Session["PropertiesDictionary"] = new Dictionary<string, string>();
                return this.Session["PropertiesDictionary"] as Dictionary<string, string>;
            }
        }

        #endregion

        #region Session && Session  getter/setter

        private void SetSessionsToDefault()
        {
            // Reset the dropdown list in the serachHeader for autocomplete
            Session["FilterAC"] = null;

            // Reset the selected index of the dropdown list in the serachHeader for autocomplete
            Session["SelectedIndexFilterAC"] = 0;

            // Reset all selected properties
            Session["PropertiesDictionary"] = null;

            // Reset The search type
            // Options : 1. new search (new) 2. based on previos search(basedon)
            Session["SearchType"] = "basedon";

            Session["resultView"] = "grid";

            Session["SelectAbleCategories"] = null;

            Session["SelectedFacets"] = null;
        }

        private string GetParentOfSelectAbleCategories()
        {
            if (Session["ParentOfSelectAbleCategories"] != null) return (string)Session["ParentOfSelectAbleCategories"];
            else return "";
        }

        private void SetParentOfSelectAbleCategories(string cl)
        {
            Session["ParentOfSelectAbleCategories"] = cl;
        }

        private List<Facet> GetSelectAbleCategoryList()
        {
            if (Session["SelectAbleCategories"] != null) return (List<Facet>)Session["SelectAbleCategories"];
            else return new List<Facet>();
        }


        private void SetSelectAbleCategoryList(IEnumerable<Facet> cl)
        {
            Session["SelectAbleCategories"] = cl;
        }

        private string GetFilterAC()
        {
            if (Session["FilterAC"] != null) return (string)Session["FilterAC"];
            else return "all";
        }

        private void SetFilterAC(string filter)
        {
            Session["FilterAC"] = filter;
        }

        private string GetSearchType()
        {
            if (Session["SearchType"] != null) return (string)Session["SearchType"];
            else return "new";
        }

        private void SetSearchType(string filter)
        {
            Session["SearchType"] = filter;
        }

        #endregion

        #region SearchDesigner


        public ActionResult SearchDesigner()
        {
            if (Session["searchAttributeList"] == null)
            {
                ISearchDesigner sd = new SearchDesigner();
     
                Session["searchAttributeList"] = GetListOfSearchAttributeViewModels(sd.Get());
                Session["metadatNodes"] = sd.GetMetadataNodes();
                ViewData["windowVisible"] = false;
            }

            return View((List<SearchAttributeViewModel>)Session["searchAttributeList"]);
        }

        [GridAction]
        public ActionResult _CustomSearchDesignerGridBinding(GridCommand command)
        {
            if (Session["searchAttributeList"] == null)
            {
                ISearchDesigner sd = new SearchDesigner();

                Session["searchAttributeList"] = GetListOfSearchAttributeViewModels(sd.Get());
                Session["metadatNodes"] = sd.GetMetadataNodes();
                ViewData["windowVisible"] = false;
            }

            return View("SearchDesigner", new GridModel((List<SearchAttributeViewModel>)Session["searchAttributeList"]));
            //return View("SearchDesigner", (List<SearchAttribute>)Session["searchAttributeList"]);
        }
    
        public ActionResult Add()
        {
            List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];

            SearchAttributeViewModel sa = new SearchAttributeViewModel();
            sa.id = searchAttributeList.Count;

            ViewData["windowVisible"] = true;
            ViewData["selectedSearchAttribute"] = sa;
            return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
        }

        public ActionResult CloseWindow()
        {

            ViewData["windowVisible"] = false;

            return Content("");
        }

        public ActionResult Edit(int id)
        {
            List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];

            ViewData["windowVisible"] = true;
            ViewData["selectedSearchAttribute"] = searchAttributeList.Where(p => p.id.Equals(id)).First();
            return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
            //return PartialView("_editSearchAttribute", searchAttributeList.Where(p => p.id.Equals(id)).First());
        }

        public ActionResult Save(string submit, SearchAttributeViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (submit != null)
                {
                    List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];

                    if (searchAttributeList.Where(p => p.id.Equals(model.id)).Count()>0)
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

        public ActionResult SaveConfig()
        {

            if (Session["searchAttributeList"] != null)
            {

                List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];
                ISearchDesigner sd = new SearchDesigner();
                sd.Set(GetListOfSearchAttributes(searchAttributeList));
                Session["searchAttributeList"] = searchAttributeList;
                ViewData["windowVisible"] = false;

                sd.Reload();

                searchConfigFileInUse = false;
            }

            return View("SearchDesigner", (List<SearchAttributeViewModel>)Session["searchAttributeList"]);
        }

        public ActionResult ResetConfig()
        {
                ISearchDesigner sd = new SearchDesigner();
                sd.Reset();
                Session["searchAttributeList"] = GetListOfSearchAttributeViewModels(sd.Get());
                Session["metadatNodes"] = sd.GetMetadataNodes();
                ViewData["windowVisible"] = false;

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


        public ActionResult Delete(int id)
        {
            List<SearchAttributeViewModel> searchAttributeList = (List<SearchAttributeViewModel>)Session["searchAttributeList"];
            searchAttributeList.Remove(searchAttributeList.Where(p => p.id.Equals(id)).First());
            Session["searchAttributeList"] = searchAttributeList;
            ViewData["windowVisible"] = false;

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

            #region ReIndex

            public ActionResult ReIndexSearch()
            {
                return View();
            }

            public ActionResult RefreshSearch()
            {
                ISearchDesigner sd = new SearchDesigner();
                sd.Reload();

                return View("ReIndexSearch");
            }
        

            #endregion

        #endregion

    }
}
