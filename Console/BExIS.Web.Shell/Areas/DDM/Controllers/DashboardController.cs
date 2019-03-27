using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Models;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Telerik.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DashboardController : Controller
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Dashboard", this.Session.GetTenant());

            DashboardModel model = GetDefaultDashboardModel();

            #region mydatasetmodel

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
                Name = "Read",
                DisplayName = "Read",
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
                Name = "Write",
                DisplayName = "Write",
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
                Name = "Grant",
                DisplayName = "Grant",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            ViewData["DefaultHeaderList"] = headerItems;

            #endregion header

            model.MyDatasets = CreateDataTable(headerItems);

            #endregion mydatasetmodel

            return View(model);
        }

        private DashboardModel GetDefaultDashboardModel()
        {
            //load entites
            DashboardModel model = new DashboardModel();

            using (var uow = this.GetUnitOfWork())
            {
                // ToDo filter entites by metadata or security.
                // maybe not all enitites in the table are requestable
                var enitites = uow.GetReadOnlyRepository<Entity>().Get();

                foreach (var entity in enitites)
                {
                    model.Entities.Add(entity.Id, entity.Name);
                }
            }

            return model;
        }

        #region mydatasets

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

            List<HeaderItem> headerItems = CreateHeaderItems();
            ViewData["DefaultHeaderList"] = headerItems;

            #endregion header

            model = CreateDataTable(headerItems);

            DatasetManager datasetManager = new DatasetManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            UserManager userManager = new UserManager();
            EntityManager entityManager = new EntityManager();

            try
            {
                var entity = entityManager.FindByName("Dataset");
                var user = userManager.FindByNameAsync(GetUsernameOrDefault()).Result;

                List<long> gridCommands = datasetManager.GetDatasetLatestIds();
                gridCommands.Skip(Convert.ToInt16(ViewData["CurrentPage"])).Take(Convert.ToInt16(ViewData["PageSize"]));

                foreach (long datasetId in gridCommands)
                {
                    //get permissions
                    int rights = entityPermissionManager.GetEffectiveRights(user?.Id, entity.Id, datasetId);

                    if (rights > 0)
                    {
                        DataRow dataRow = model.NewRow();
                        Object[] rowArray = new Object[7];

                        if (datasetManager.IsDatasetCheckedIn(datasetId))
                        {
                            //long versionId = datasetManager.GetDatasetLatestVersionId (datasetId); // check for zero value
                            //DatasetVersion dsv = datasetManager.DatasetVersionRepo.Get(versionId);

                            DatasetVersion dsv = datasetManager.GetDatasetLatestVersion(datasetId);

                            //MetadataStructureManager msm = new MetadataStructureManager();
                            //dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

                            string title = xmlDatasetHelper.GetInformationFromVersion(dsv.Id, NameAttributeValues.title);
                            string description = xmlDatasetHelper.GetInformationFromVersion(dsv.Id, NameAttributeValues.description);

                            rowArray[0] = Convert.ToInt64(datasetId);
                            rowArray[1] = title;
                            rowArray[2] = description;
                        }
                        else
                        {
                            rowArray[0] = Convert.ToInt64(datasetId);
                            rowArray[1] = "";
                            rowArray[2] = "Dataset is just in processing.";
                        }

                        rowArray[3] = (rights & (int)RightType.Read) > 0 ? "✔" : "✘";
                        rowArray[4] = (rights & (int)RightType.Write) > 0 ? "✔" : "✘";
                        rowArray[5] = (rights & (int)RightType.Delete) > 0 ? "✔" : "✘";
                        //rowArray[6] = (rights & (int)RightType.Download) > 0 ? "✔" : "✘";
                        rowArray[6] = (rights & (int)RightType.Grant) > 0 ? "✔" : "✘";

                        dataRow = model.NewRow();
                        dataRow.ItemArray = rowArray;
                        model.Rows.Add(dataRow);
                    }
                }

                return View(new GridModel(model));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                datasetManager.Dispose();
                entityPermissionManager.Dispose();
                entityManager.Dispose();
                userManager.Dispose();
            }
        }

        /// <summary>
        /// create the model of My Dataset table
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="_CustomMyDatasetBinding"/>
        /// <param>NA</param>
        /// <returns>model</returns>
        public ActionResult ShowMyDatasets()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Dashboard", this.Session.GetTenant());

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
                Name = "Read",
                DisplayName = "Read",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            headerItem = new HeaderItem()
            {
                Name = "Write",
                DisplayName = "Write",
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
                Name = "Grant",
                DisplayName = "Grant",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            ViewData["DefaultHeaderList"] = headerItems;

            #endregion header

            model = CreateDataTable(headerItems);

            return PartialView("_myDatasetGridView", model);
        }

        /// <summary>
        /// create the model of My Dataset table
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="_CustomMyDatasetBinding"/>
        /// <param>NA</param>
        /// <returns>model</returns>
        public ActionResult ShowMyDatasetsInFullPage()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Dashboard", this.Session.GetTenant());

            DataTable model = new DataTable();

            ViewData["PageSize"] = 10;
            ViewData["CurrentPage"] = 1;

            #region header

            List<HeaderItem> headerItems = CreateHeaderItems();

            ViewData["DefaultHeaderList"] = headerItems;

            #endregion header

            model = CreateDataTable(headerItems);

            return View("_myDatasetGridView", model);
        }

        private List<HeaderItem> CreateHeaderItems()
        {
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
                Name = "Read",
                DisplayName = "Read",
                DataType = "String"
            };
            headerItems.Add(headerItem);


            headerItem = new HeaderItem()
            {
                Name = "Write",
                DisplayName = "Write",
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
                Name = "Grant",
                DisplayName = "Grant",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            return headerItems;
        }

        private DataTable CreateDataTable(List<HeaderItem> items)
        {
            DataTable table = new DataTable();

            foreach (HeaderItem item in items)
            {
                table.Columns.Add(new DataColumn()
                {
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
        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        #endregion mydatasets

        #region requests & decisions

        /// <summary>
        /// load requests of a entity type & user
        /// </summary>
        /// <param name="id">Id of a entity type</param>
        /// <returns></returns>
        public ActionResult Requests(long id)
        {
            if (this.IsAccessible("SAM", "Requests", "Requests"))
            {
                var view = this.Render("SAM", "Requests", "Requests", new RouteValueDictionary()
                {
                    { "entityId", id }
                });

                return Content(view.ToHtmlString(), "text/html");
            }

            return PartialView("Error"); ;
        }

        public ActionResult Decisions(long id)
        {
            if (this.IsAccessible("SAM", "Requests", "Decisions"))
            {
                var view = this.Render("SAM", "Requests", "Decisions", new RouteValueDictionary()
                {
                    { "entityId", id }
                });

                return Content(view.ToHtmlString(), "text/html");
            }

            return PartialView("Error"); ;
        }

        #endregion requests & decisions
    }
}