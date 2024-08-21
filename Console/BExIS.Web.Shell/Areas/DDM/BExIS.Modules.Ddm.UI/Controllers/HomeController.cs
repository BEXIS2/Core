using BExIS.Ddm.Api;
using BExIS.Utils.Models;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.IoC;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using SearchCriteria = BExIS.Utils.Models.SearchCriteria;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class HomeController : Controller
    {
        public bool searchConfigFileInUse = false;

        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        /// <summary>
        /// is called when the Search View is selected
        ///
        /// </summary>
        /// <param name="model">from type SearchDataModel</param>
        /// <returns></returns>
        //[Authorize(Roles="Guest")]
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Search", this.Session.GetTenant());
            Session["SubmissionAction"] = "Index";
            Session["Controller"] = "Home";
            Session["PropertiesDictionary"] = null;

            try
            {
                ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

                provider.WorkingSearchModel.CriteriaComponent.Clear();
                provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

                SetSessionsToDefault();

                return View(provider);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);

                return View("Error");
            }
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
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Search", this.Session.GetTenant());
            Session["SubmissionAction"] = "Index";
            Session["Controller"] = "Home";

            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

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
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Search", this.Session.GetTenant());
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
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
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            SearchModel model = provider.GetTextBoxSearchValues(text, GetFilterAC(), Session["SearchType"].ToString(), 10);
            IEnumerable<TextValue> textvalues = model.SearchComponent.TextBoxSearchValues;

            return new JsonResult { Data = new SelectList(textvalues, "Value", "Name") };
        }

        /// <summary>
        /// Is called when the user change the SearchType
        /// </summary>
        /// <param name="value">consist the searchType</param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public void ChangeSearchValuesACBySearchType(string value)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            Session["SearchType"] = value;
        }

        #endregion SearchHeader

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
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            provider.WorkingSearchModel.UpdateSearchCriteria(Parent, SelectedItem, SearchComponentBaseType.Facet, true);
            provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

            return PartialView("_searchFacets", Tuple.Create(provider.WorkingSearchModel, provider.DefaultSearchModel.SearchComponent.Facets));
        }

        public ActionResult UpdateFacets()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            return PartialView("_searchFacets", Tuple.Create(provider.UpdateFacets(provider.WorkingSearchModel.CriteriaComponent), provider.DefaultSearchModel.SearchComponent.Facets));
        }

        public ActionResult UpdateProperties()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            var properties = provider.UpdateProperties(provider.WorkingSearchModel.CriteriaComponent).SearchComponent.Properties;

            foreach (Property p in properties)
            {
                if (PropertiesDic.ContainsKey(p.DataSourceKey))
                {
                    p.SelectedValue = PropertiesDic[p.DataSourceKey];
                }
                else
                {
                    if (!string.IsNullOrEmpty(p.SelectedValue)) p.SelectedValue = string.Empty;
                }
            }

            return PartialView("_searchProperties", properties);
        }

        public ActionResult GetDataForBreadCrumbView()
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

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
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Search", this.Session.GetTenant());

            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
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
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Search", this.Session.GetTenant());

            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

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

            return View("Index", provider);
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
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

            SetParentOfSelectAbleCategories(parent);

            var facet = provider.WorkingSearchModel.SearchComponent.Facets.Where(p => p.Name.Equals(parent, StringComparison.InvariantCulture)).FirstOrDefault();

            SetParentOfSelectAbleCategories(parent);

            // List<Facet> sortedList = facet.Childrens.OrderBy(p => p.DisplayName).ToList();

            SetSelectAbleCategoryList(facet.Childrens.Where(p => p.Count > 0).OrderBy(p => p.Name.ToLower()).ToList());

            return PartialView("_windowCheckBoxList", provider.WorkingSearchModel);
        }

        #endregion Treeview - _searchFacets

        #region BreadcrumbView

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
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Search", this.Session.GetTenant());

            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

            RemoveFromPropertiesDic(parent, value, provider.WorkingSearchModel.CriteriaComponent);

            // if value exist / user clicked value
            if (value != null && parent != null)
            {
                provider.WorkingSearchModel.CriteriaComponent.RemoveValueOfSearchCriteria(parent, value);
            }

            provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);

            //reset properties selected values
            var properties = provider.WorkingSearchModel.SearchComponent.Properties;

            foreach (Property p in properties)
            {
                if (PropertiesDic.ContainsKey(p.DataSourceKey))
                {
                    p.SelectedValue = PropertiesDic[p.DataSourceKey];
                }
                else
                {
                    if (!string.IsNullOrEmpty(p.SelectedValue)) p.SelectedValue = string.Empty;
                }
            }

            return View(Session["SubmissionAction"].ToString(), provider); //View("Index", provider);
        }

        #endregion BreadcrumbView

        #region Datagrid

        // +++++++++++++++++++++ DataGRID Action +++++++++++++++++++++++++++

        [GridAction]
        public ActionResult _CustomBinding(GridCommand command)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            DataTable table = provider.Get(provider.WorkingSearchModel.CriteriaComponent).ResultComponent.ConvertToDataTable();

            return View(new GridModel(table));
        }

        public ActionResult SetResultViewVar(string key, string value)
        {
            Session[key] = value;

            return this.Json(new { success = true });
        }

        #endregion Datagrid

        #region Properties _searchProperties

        //+++++++++++++++++++++ Properties Sliders Action +++++++++++++++++++++++++++
        [HttpPost]
        public ActionResult FilterByRangeSlider(int start, int end, string parent)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            return PartialView("_searchBreadcrumb", provider.WorkingSearchModel);
        }

        [HttpPost]
        public ActionResult FilterBySlider(int value, string parent)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

            UpdatePropertiesDic(parent, value.ToString());
            provider.WorkingSearchModel.UpdateSearchCriteria(parent, value.ToString(), SearchComponentBaseType.Property, false, true);

            return PartialView("_searchBreadcrumb", provider.Get(provider.WorkingSearchModel.CriteriaComponent));
        }

        //+++++++++++++++++++++Properties DropDown Action +++++++++++++++++++++++++++
        [HttpPost]
        public ActionResult FilterByDropDown(string value, string node)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

            UpdatePropertiesDic(node, value);
            provider.WorkingSearchModel.UpdateSearchCriteria(node, value.ToString(), SearchComponentBaseType.Property);

            return PartialView("_searchBreadcrumb", provider.Get(provider.WorkingSearchModel.CriteriaComponent, 10, 1));
        }

        //+++++++++++++++++++++Properties RadioButton Action +++++++++++++++++++++++++++
        [HttpPost]
        public ActionResult FilterByRadioButton(string value, string node, bool isChecked)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();
            UpdatePropertiesDic(node, value);
            provider.WorkingSearchModel.UpdateSearchCriteria(node, value.ToString(), SearchComponentBaseType.Property);

            return PartialView("_searchBreadcrumb", provider.Get(provider.WorkingSearchModel.CriteriaComponent));
        }

        //+++++++++++++++++++++Properties ´CheckButton Action +++++++++++++++++++++++++++

        [HttpPost]
        public ActionResult FilterByCheckBox(string value, string node, bool isChecked)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>();

            UpdatePropertiesDic(node, value);
            provider.WorkingSearchModel.UpdateSearchCriteria(node, value.ToString(), SearchComponentBaseType.Property, true);

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
        /// </summary>
        /// <example>
        /// grassland: all | yes | no
        /// grassland is name
        /// value is (all | yes | no)
        /// </example>
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

        #endregion Properties _searchProperties

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

        #endregion Dictionary (Search/Properties)

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

        #endregion Session && Session  getter/setter
    }
}