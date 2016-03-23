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
using BExIS.Ddm.Api;
using BExIS.Ddm.Model;
using BExIS.Ddm.Providers.LuceneProvider;
using BExIS.Web.Shell.Areas.DDM.Helpers;
using BExIS.Web.Shell.Areas.DDM.Models;
using Telerik.Web.Mvc;
using Vaiona.IoC;
using Vaiona.Utils.Cfg;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Entities.Objects;
using System.Xml;
using BExIS.Xml.Services;
using BExIS.Dlm.Services.MetadataStructure;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.DDM.Controllers
{
    public class HomeController : Controller
    {
        public bool searchConfigFileInUse = false;

        /// <summary>
        /// is called when the Search View is selected
        /// 
        /// </summary>
        /// <param name="model">from type SearchDataModel</param>
        /// <returns></returns>
        //[Authorize(Roles="Guest")]
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Search");

            try
            {
                ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;

                //if (provider.WorkingSearchModel.CriteriaComponent.SearchCriteriaList.Count > 0)
                //{
                    provider.WorkingSearchModel.CriteriaComponent.Clear();
                    provider.SearchAndUpdate(provider.WorkingSearchModel.CriteriaComponent);
                //}
                //var pp = IoCFactory.Container.ResolveAll<ISearchProvider>();

                SetSessionsToDefault();

                return View(provider);
            }
            catch(Exception e)
            {
                ModelState.AddModelError(String.Empty, e.Message);

                return View();
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
            ViewBag.Title = PresentationModel.GetViewTitle("Search");

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
        public void ChangeSearchValuesACBySearchType(string value)
        {
            ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
            Session["SearchType"] = value;
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

            SetSelectAbleCategoryList(facet.Childrens.OrderBy(p => p.Name.ToLower()).ToList());

            return PartialView("_windowCheckBoxList", provider.WorkingSearchModel);
        }

        #endregion
        
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

        #region mydatasets
        
        /// <summary>
        /// create the model of My Dataset table
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="_CustomMyDatasetBinding"/>
        /// <param>NA</param>       
        /// <returns>model</returns>
        public ActionResult ShowMyDatasets()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Dashboard");

                DataTable model = new DataTable();

                ViewData["PageSize"] = 10;
                ViewData["CurrentPage"] = 1;


                #region header
                List<HeaderItem> headerItems = new List<HeaderItem>();


                HeaderItem headerItem = new HeaderItem()
                {
                    Name = "ID",
                    DisplayName = "ID",
                    DataType = "Int64"
                };
                headerItems.Add(headerItem);

                ViewData["Id"] = headerItem;

                headerItem = new HeaderItem()
                {
                    Name = "Title",
                    DisplayName = "Title",
                    DataType = "String"
                };
                headerItems.Add(headerItem);

                headerItem = new HeaderItem()
                {
                    Name = "Description",
                    DisplayName = "Description",
                    DataType = "String"
                };
                headerItems.Add(headerItem);

                headerItem = new HeaderItem()
                {
                    Name = "View",
                    DisplayName = "View",
                    DataType = "String"
                };
                headerItems.Add(headerItem);

                headerItem = new HeaderItem()
                {
                    Name = "Update",
                    DisplayName = "Update",
                    DataType = "String"
                };
                headerItems.Add(headerItem);

                headerItem = new HeaderItem()
                {
                    Name = "Delete",
                    DisplayName = "Delete",
                    DataType = "String"
                };
                headerItems.Add(headerItem);

                headerItem = new HeaderItem()
                {
                    Name = "Download",
                    DisplayName = "Download",
                    DataType = "String"
                };
                headerItems.Add(headerItem);

                headerItem = new HeaderItem()
                {
                    Name = "Grant",
                    DisplayName = "Grant",
                    DataType = "String"
                };
                headerItems.Add(headerItem);

                ViewData["DefaultHeaderList"] = headerItems;

                #endregion

                model = CreateDataTable(headerItems);

                return PartialView("_myDatasetGridView", model);
        }

        [GridAction]
        /// <summary>
        /// create a model to fill the table of My Dataset
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="ShowMyDatasets"/>
        /// <param>NA</param>       
        /// <returns>model</returns>
        public ActionResult _CustomMyDatasetBinding()
        {
            DataTable model = new DataTable();

            ViewData["PageSize"] = 10;
            ViewData["CurrentPage"] = 1;

            #region header
            List<HeaderItem> headerItems = new List<HeaderItem>();

            HeaderItem headerItem = new HeaderItem()
            {
                Name = "ID",
                DisplayName = "ID",
                DataType = "Int64"
            };
            headerItems.Add(headerItem);

            ViewData["Id"] = headerItem;

            headerItem = new HeaderItem()
            {
                Name = "Title",
                DisplayName = "Title",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            headerItem = new HeaderItem()
            {
                Name = "Description",
                DisplayName = "Description",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            headerItem = new HeaderItem()
            {
                Name = "View",
                DisplayName = "View",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            headerItem = new HeaderItem()
            {
                Name = "Update",
                DisplayName = "Update",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            headerItem = new HeaderItem()
            {
                Name = "Delete",
                DisplayName = "Delete",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            headerItem = new HeaderItem()
            {
                Name = "Download",
                DisplayName = "Download",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            headerItem = new HeaderItem()
            {
                Name = "Grant",
                DisplayName = "Grant",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            ViewData["DefaultHeaderList"] = headerItems;

            #endregion

            model = CreateDataTable(headerItems);

            DatasetManager datasetManager = new DatasetManager();
            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            foreach (long datasetId in datasetManager.GetDatasetLatestIds())
            {
                //get permissions
                List<int> rights = permissionManager.GetAllRights(subjectManager.GetUserByName(GetUserNameOrDefault()).Id, 1, datasetId).ToList();

                if (rights.Count > 0)
                {
                    DatasetVersion dsv = datasetManager.GetDatasetLatestVersion(datasetId);

                    MetadataStructureManager msm = new MetadataStructureManager();
                    dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                    string title = XmlDatasetHelper.GetInformation(dsv, AttributeNames.title);
                    string description = XmlDatasetHelper.GetInformation(dsv, AttributeNames.description);

                    DataRow dataRow = model.NewRow();
                    Object[] rowArray = new Object[8];

                    rowArray[0] = Convert.ToInt64(datasetId);
                    rowArray[1] = title;
                    rowArray[2] = description;

                    if (rights.Contains(1)) { rowArray[3] = "✔"; } else { rowArray[3] = "✘"; }
                    if (rights.Contains(2)) { rowArray[4] = "✔"; } else { rowArray[4] = "✘"; }
                    if (rights.Contains(3)) { rowArray[5] = "✔"; } else { rowArray[5] = "✘"; }
                    if (rights.Contains(4)) { rowArray[6] = "✔"; } else { rowArray[6] = "✘"; }
                    if (rights.Contains(5)) { rowArray[7] = "✔"; } else { rowArray[7] = "✘"; }

                    dataRow = model.NewRow();
                    dataRow.ItemArray = rowArray;
                    model.Rows.Add(dataRow);
                }
            }

            return View(new GridModel(model));
        }

 
        private DataTable CreateDataTable(List<HeaderItem> items)
        {
            DataTable table = new DataTable();

            foreach (HeaderItem item in items)
            {
                table.Columns.Add(new DataColumn(){
                    ColumnName = item.Name,
                    Caption = item.DisplayName,
                    DataType = getDataType(item.DataType)
                });
            }

            return table;
        }

        private Type getDataType(string dataType)
        {
            switch (dataType)
            {
                case "String":
                    {
                        return Type.GetType("System.String");
                    }

                case "Double":
                    {
                        return Type.GetType("System.Double");
                    }

                case "Int16":
                    {
                        return Type.GetType("System.Int16");
                    }

                case "Int32":
                    {
                        return Type.GetType("System.Int32");
                    }

                case "Int64":
                    {
                        return Type.GetType("System.Int64");
                    }

                case "Decimal":
                    {
                        return Type.GetType("System.Decimal");
                    }

                case "DateTime":
                    {
                        return Type.GetType("System.DateTime");
                    }

                default:
                    {
                        return Type.GetType("System.String");
                    }
            }
        }

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
        public string GetUserNameOrDefault()
        {
            string userName = string.Empty;
            try
            {
                userName = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
        }

        #endregion


    }
}
