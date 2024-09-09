using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
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

            // load settings
            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            ViewData["use_tags"] = moduleSettings.GetValueByKey("use_tags");
            ViewData["use_minor"] = moduleSettings.GetValueByKey("use_minor");


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

            headerItem = new HeaderItem()
            {
                Name = "Valid",
                DisplayName = "is valid",
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

                List<DatasetVersion> datasetVersions = datasetManager.GetDatasetLatestVersions(gridCommands, false);
                foreach (var dsv in datasetVersions)
                {
                    var datasetId = dsv.Dataset.Id;

                    //get permissions
                    int rights = entityPermissionManager.GetEffectiveRightsAsync(user.Id, entity.Id, datasetId).Result;

                    if (rights > 0)
                    {
                        DataRow dataRow = model.NewRow();
                        Object[] rowArray = new Object[8];
                        string isValid = "no";

                        if (datasetManager.IsDatasetCheckedIn(datasetId))
                        {
                            string title = dsv.Title;
                            string description = dsv.Description;

                            if (dsv.StateInfo != null)
                            {
                                isValid = DatasetStateInfo.Valid.ToString().Equals(dsv.StateInfo.State) ? "yes" : "no";
                            }

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
                        rowArray[7] = isValid;

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
        /// <param name="entityname">Name of entity</param>
        /// <param name="rightType">Type of right (write, delete, grant, read)</param>
        /// <param name="onlyTable">Return only table without header</param>
        /// <returns>model</returns>
        public ActionResult ShowMyDatasets(string entityname, RightType rightType, string onlyTable = "false")
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Dashboard", this.Session.GetTenant());

            List<MyDatasetsModel> model = new List<MyDatasetsModel>();

            // load settings
            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            ViewData["use_tags"] = moduleSettings.GetValueByKey("use_tags");
            ViewData["use_minor"] = moduleSettings.GetValueByKey("use_minor");

            using (DatasetManager datasetManager = new DatasetManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (UserManager userManager = new UserManager())
            using (EntityManager entityManager = new EntityManager())
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (PartyManager partyManager = new PartyManager())
            {
                // Entity, Entity Party Type and Entity Party
                var entity = entityManager.FindByName(entityname);
                var entityPartyType = partyTypeManager.PartyTypeRepository.Get(p => p.Title == entity.Name).FirstOrDefault();
                var entityPartyIds = partyManager.Parties.Where(p => p.PartyType.Id == entityPartyType.Id).Select(p => p.Id).ToList();

                List<long> datasetIds = new List<long>();

                // get user
                var user = userManager.FindByNameAsync(GetUsernameOrDefault()).Result;

                if (user != null)
                {
                    ViewBag.userLoggedIn = true;

                    // get datasets based on entity permissions
                    datasetIds = entityPermissionManager.GetKeys(GetUsernameOrDefault(), entityname, typeof(Dataset), rightType).Result;

                    var userParty = partyManager.GetPartyByUser(user.Id);

                    if (userParty != null)
                    {
                        // get datasets based on party relationships
                        List<long> partyIds = partyManager.PartyRelationshipRepository.Get(p => p.SourceParty.Id == userParty.Id && p.Permission >= (int)rightType).Select(p => p.TargetParty.Id).ToList();

                        foreach (var partyId in partyIds)
                        {
                            if (entityPartyIds.Contains(partyId))
                            {
                                long datasetId = 0;
                                var success = long.TryParse(partyManager.Find(partyId).Name, out datasetId);
                                if (success && !datasetIds.Contains(datasetId))
                                {
                                    datasetIds.Add(datasetId);
                                }
                            }
                        }
                    }
                }
                else
                {
                    ViewBag.userLoggedIn = false;
                    datasetIds = entityPermissionManager.GetKeys(GetUsernameOrDefault(), entityname, typeof(Dataset), RightType.Read).Result;
                }

                List<DatasetVersion> datasetVersions = datasetManager.GetDatasetLatestVersions(datasetIds, true);
                foreach (var dsv in datasetVersions)
                {
                    Object[] rowArray = new Object[8];
                    string isValid = "no";

                    string type = "file";
                    if (dsv.Dataset.DataStructure?.Self is StructuredDataStructure)
                    {
                        type = "tabular";
                    }

                    if (dsv.Dataset.Status == DatasetStatus.CheckedIn)
                    {
                        string title = dsv.Title;
                        string description = dsv.Description;

                        if (dsv.StateInfo != null)
                        {
                            isValid = DatasetStateInfo.Valid.ToString().Equals(dsv.StateInfo.State) ? "yes" : "no";
                        }

                        rowArray[0] = Convert.ToInt64(dsv.Dataset.Id);
                        rowArray[1] = title;
                        rowArray[2] = description;
                        rowArray[3] = type;
                    }
                    else
                    {
                        rowArray[0] = Convert.ToInt64(dsv.Dataset.Id);
                        rowArray[1] = "";
                        rowArray[2] = "Dataset is just in processing.";
                        rowArray[3] = type;
                    }

                    //
                    rowArray[7] = entityPermissionManager.HasEffectiveRightsAsync(HttpContext.User.Identity.Name, typeof(Dataset), dsv.Dataset.Id, RightType.Write).Result;

                    model.Add(new MyDatasetsModel(
                       (long)rowArray[0],
                      (string)rowArray[1],
                       (string)rowArray[2],
                       (bool)rowArray[7],
                       isValid, (string)rowArray[3]));
                }
            }
            if (onlyTable == "true")
            {
                return PartialView("_myDatasetsView", model);
            }
            else
            {
                ViewBag.entityname = entityname;
                return PartialView("_myDatasetsViewHeader", model);
            }
        }

        /// <summary>
        /// create the model of My Dataset table
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="_CustomMyDatasetBinding"/>
        /// <param>NA</param>
        /// <returns>model</returns>
        public ActionResult ShowMyDatasets_old()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Dashboard", this.Session.GetTenant());

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

            headerItem = new HeaderItem()
            {
                Name = "Valid",
                DisplayName = "is valid",
                DataType = "String"
            };
            headerItems.Add(headerItem);

            ViewData["DefaultHeaderList"] = headerItems;

            #endregion header

            using (DataTable model = CreateDataTable(headerItems))
            {
                ViewData["PageSize"] = 10;
                ViewData["CurrentPage"] = 1;

                return PartialView("_myDatasetGridView", model);
            }
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

            headerItem = new HeaderItem()
            {
                Name = "Valid",
                DisplayName = "is valid",
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
    }
}