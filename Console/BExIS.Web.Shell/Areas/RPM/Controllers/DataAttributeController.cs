using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.Web.Shell.Areas.RPM.Models;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class DataAttributeController : Controller
    {
        //
        // GET: /RPM/DataType/

        public ActionResult Index()
        {
            return AttributeManager();
        }

        public bool attributeInUse(DataAttribute attribute)
        {
            if (attribute.UsagesAsVariable.Count() == 0 && attribute.UsagesAsParameter.Count() == 0)
                return false;
            else
                return true;
        }

        public ActionResult AttributeManager(DataAttributeManagerModel dataAttributeManagerModel = null)
        {
            if (Session["Window"] == null)
                Session["Window"] = false;
            if(dataAttributeManagerModel == null)
                return View(new DataAttributeManagerModel());
            else
                return View(dataAttributeManagerModel);
        }

        public ActionResult editAttribute(DataAttributeModel Model)
        {
            DataContainerManager dataAttributeManager = new DataContainerManager();
            IList<DataAttribute> DataAttributeList = dataAttributeManager.DataAttributeRepo.Get();
            long tempUnitId = Convert.ToInt64(Model.Unit.Id);
            long tempDataTypeId = Convert.ToInt64(Model.DataType.Id);

            Model.Id = Model.Id;
            Model.ShortName = cutSpaces(Model.ShortName);
            Model.Name = cutSpaces(Model.Name);
            Model.Description = cutSpaces(Model.Description);

            //if (Model.DomainConstraints.Count > 0)
            //{
            //    if (Model.DomainConstraints. != null && Model.DomainItems.Count > 0)
            //    {
            //        Model.DomainConstraints.FirstOrDefault().DomainItems = clearEmptyItems(Model.DomainItems);
            //    }
            //}


            if (Model.Name == "" | Model.Name == null)
            {
                

                Session["nameMsg"] = "invalid Name";
                Session["Window"] = true;
                return View("AttributeManager", new DataAttributeManagerModel(Model));
            }
            else
            {
                bool nameNotExist = DataAttributeList.Where(p => p.Name.ToLower().Equals(Model.Name.ToLower())).Count().Equals(0);

                if (Model.Id == 0)
                {
                    if (nameNotExist)
                    {
                        UnitManager UM = new UnitManager();
                        Unit unit = UM.Repo.Get(tempUnitId);
                        DataTypeManager DTM = new DataTypeManager();
                        DataType dataType = DTM.Repo.Get(tempDataTypeId);
                        DataContainerManager DAM = new DataContainerManager();

                        DataAttribute temp = new DataAttribute();
                        temp = DAM.CreateDataAttribute(Model.ShortName, Model.Name, Model.Description, false, false, "", MeasurementScale.Categorial, DataContainerType.ReferenceType, "", dataType, unit, null, null, null, null, null, null);
                    
                    
                        #region store constraint

                        if (Model.RangeConstraints.Count > 0 && (Model.RangeConstraints.FirstOrDefault().Min !=null || Model.RangeConstraints.FirstOrDefault().Max !=null)
                            && (Model.RangeConstraints.FirstOrDefault().Min != 0.0 || Model.RangeConstraints.FirstOrDefault().Max != 0.0 ))
                            temp = storeConstraint(Model.RangeConstraints.First(), temp);

                        if (Model.PatternConstraints.Count > 0 && !String.IsNullOrEmpty(Model.PatternConstraints.FirstOrDefault().MatchingPhrase))
                            temp = storeConstraint(Model.PatternConstraints.First(), temp);

                        if (Model.DomainConstraints.Count > 0)
                        {
                            foreach (DomainConstraintModel d in Model.DomainConstraints)
                            {
                                temp = storeConstraint(d, temp);
                            }
                        }

                        #endregion


                        temp = DAM.UpdateDataAttribute(temp);
                    }
                    else
                    {
                        Session["nameMsg"] = "Name already exist";
                        Session["Window"] = true;
                        return View("AttributeManager", new DataAttributeManagerModel(Model));
                    }
                }
                else
                {
                    if (nameNotExist || DataAttributeList.Where(p => p.Name.Equals(Model.Name)).ToList().First().Id == Model.Id)
                    {
                        DataAttribute dataAttribute = DataAttributeList.Where(p => p.Id.Equals(Model.Id)).ToList().First();
                        if (!attributeInUse(dataAttribute))
                        {
                            DataContainerManager DAM = new DataContainerManager();
                            dataAttribute.Name = cutSpaces(Model.Name);
                            dataAttribute.ShortName = Model.ShortName;
                            dataAttribute.Description = Model.Description;
                            UnitManager UM = new UnitManager();
                            dataAttribute.Unit = UM.Repo.Get(tempUnitId);
                            DataTypeManager DTM = new DataTypeManager();
                            dataAttribute.DataType = DTM.Repo.Get(tempDataTypeId);

                            #region store constraint

                            if (Model.RangeConstraints.Count > 0 && (Model.RangeConstraints.FirstOrDefault().Min != null || Model.RangeConstraints.FirstOrDefault().Max != null)
                                && (Model.RangeConstraints.FirstOrDefault().Min != 0.0 || Model.RangeConstraints.FirstOrDefault().Max != 0.0))
                                dataAttribute = storeConstraint(Model.RangeConstraints.First(), dataAttribute);

                            if (Model.PatternConstraints.Count > 0 && !String.IsNullOrEmpty(Model.PatternConstraints.FirstOrDefault().MatchingPhrase))
                                dataAttribute = storeConstraint(Model.PatternConstraints.First(), dataAttribute);

                            if (Model.DomainConstraints.Count > 0)
                            {
                                foreach (DomainConstraintModel d in Model.DomainConstraints)
                                {
                                    dataAttribute = storeConstraint(d, dataAttribute);
                                }
                            }

                            #endregion
                            
                            DAM.UpdateDataAttribute(dataAttribute);
                        }
                    }
                    else
                    {
                        Session["nameMsg"] = "Name already exist";
                        Session["Window"] = true;
                        return View("AttributeManager", new DataAttributeManagerModel(Model));
                    }
                }
            }

            Session["Window"] = false;
            return View("AttributeManager", new DataAttributeManagerModel(Model));
        }

        public ActionResult deletAttribute(long id, string name)
        {

            if (id != 0)
            {
                DataContainerManager DAM = new DataContainerManager();
                DataAttribute dataAttribute = DAM.DataAttributeRepo.Get(id);
                if (dataAttribute != null)
                {
                    if (!attributeInUse(dataAttribute))
                    {

                        DAM.DeleteDataAttribute(dataAttribute);
                    }

                }
            }
            return RedirectToAction("AttributeManager");
        }

        public ActionResult openAttributeWindow(long id)
        {
            if (id != 0)
            {

                Session["nameMsg"] = null;
                Session["Window"] = true;
                return RedirectToAction("AttributeManager", new DataAttributeManagerModel(id));
            }
            else
            {
                Session["nameMsg"] = null;
                Session["Window"] = true;
                return RedirectToAction("AttributeManager", new DataAttributeManagerModel());
            }
        }

        public JsonResult getDatatypeList(string Id)
        {
            long tempId = Convert.ToInt64(Id);
            UnitManager unitManager = new UnitManager();
            Unit tempUnit = unitManager.Repo.Get(tempId);
            List<DataType> dataTypeList = new List<DataType>();
            if (tempUnit.Name.ToLower() == "none")
            {
                DataTypeManager dataTypeManager = new DataTypeManager();
                dataTypeList = dataTypeManager.Repo.Get().ToList();
                dataTypeList = dataTypeList.OrderBy(p => p.Name).ToList();
            }
            else
            {
                dataTypeList = unitManager.Repo.Get(tempId).AssociatedDataTypes.ToList();
                dataTypeList = dataTypeList.OrderBy(p => p.Name).ToList();
            }

            return Json(new SelectList(dataTypeList.ToArray(), "Id", "Name"), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getRangeConstraintFormalDescription(string invert, string min, string max, string mininclude, string maxinclude)
        {
            RangeConstraint temp = new RangeConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, "", Convert.ToBoolean(invert), null, null, null, Convert.ToDouble(min), Convert.ToBoolean(mininclude), Convert.ToDouble(max), Convert.ToBoolean(maxinclude));
            return Json((temp.FormalDescription), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getPatternConstraintFormalDescription(string invert, string phrase)
        {
            PatternConstraint temp = new PatternConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, "", Convert.ToBoolean(invert), null, null, null, phrase, false);
            return Json((temp.FormalDescription), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getDomainConstraintFormalDescription(string invert, string Terms)
        {
            List<DomainItem> items = createDomainItems(Terms);

            DomainConstraint Temp = new DomainConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, "", Convert.ToBoolean(invert), null, null, null, items);
            return Json((Temp.FormalDescription), JsonRequestBehavior.AllowGet);
        }

        private string cutSpaces(string str)
        {
            if (str != "" && str != null)
            {
                str = str.Trim();
                if (str.Length > 255)
                    str = str.Substring(0, 255);
            }
            return (str);
        }

        #region constraints

        public ActionResult AddDomainItem()
        {
            return PartialView("_domainItemView", new DomainConstraintItemModel());
        }

        private DataAttribute storeConstraint(ConstraintModel constraintModel, DataAttribute dataAttribute)
        {
            DataContainerManager dcManager = new DataContainerManager();
           
            if (constraintModel is RangeConstraintModel)
            {
                RangeConstraintModel rcm = (RangeConstraintModel)constraintModel;
                
                if (rcm.Id == 0)
                {
                    RangeConstraint constraint = new RangeConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, rcm.Description, rcm.Negated, null, null, null, rcm.Min, rcm.MinInclude, rcm.Max, rcm.MaxInclude);
                    dcManager.AddConstraint(constraint, dataAttribute);             
                }
                else
                { 
                    for(int i = 0; i < dataAttribute.Constraints.Count; i++)
                    {
                        if (dataAttribute.Constraints.ElementAt(i).Id == rcm.Id)
                        {
                            ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).Description = rcm.Description;
                            ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).Negated = rcm.Negated;
                            ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).Lowerbound = rcm.Min;
                            ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).LowerboundIncluded = rcm.MinInclude;
                            ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).Upperbound = rcm.Max;
                            ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).UpperboundIncluded = rcm.MaxInclude;
                        }
                    }
                }
            }

            if (constraintModel is PatternConstraintModel)
            {
                PatternConstraintModel pcm = (PatternConstraintModel)constraintModel;

                if (pcm.Id == 0)
                {
                    PatternConstraint constraint = new PatternConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, pcm.Description, pcm.Negated, null, null, null, pcm.MatchingPhrase, false);
                    dcManager.AddConstraint(constraint, dataAttribute);
                }
                else
                {
                    for (int i = 0; i < dataAttribute.Constraints.Count; i++)
                    {
                        if (dataAttribute.Constraints.ElementAt(i).Id == pcm.Id)
                        {
                            ((PatternConstraint)dataAttribute.Constraints.ElementAt(i)).Description = pcm.Description;
                            ((PatternConstraint)dataAttribute.Constraints.ElementAt(i)).Negated = pcm.Negated;
                            ((PatternConstraint)dataAttribute.Constraints.ElementAt(i)).MatchingPhrase = pcm.MatchingPhrase;
                        }
                    }
                }
            }

            if (constraintModel is DomainConstraintModel)
            {
                DomainConstraintModel dcm = (DomainConstraintModel)constraintModel;

                List<DomainItem> items = createDomainItems(dcm.Terms);

                dcm.Terms = cutSpaces(dcm.Terms);

                if (items.Count > 0)
                {
                    if (dcm.Id == 0)
                    {
                        DomainConstraint constraint = new DomainConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, dcm.Description, dcm.Negated, null, null, null, items);
                        dcManager.AddConstraint(constraint, dataAttribute);
                    }
                    else
                    {
                        DomainConstraint temp = new DomainConstraint();
                        for (int i = 0; i < dataAttribute.Constraints.Count; i++)
                        {
                            if (dataAttribute.Constraints.ElementAt(i).Id == dcm.Id)
                            {
                                temp = (DomainConstraint)dataAttribute.Constraints.ElementAt(i);
                                temp.Materialize();
                                temp.Description = dcm.Description;
                                temp.Negated = dcm.Negated;
                                temp.Items = items;
                                dcManager.AddConstraint(temp, dataAttribute);
                            }
                        }
                    }
                }                
            }

            return dataAttribute;
        }

        private List<DomainItem> createDomainItems(string Terms)
        { 
            List<DomainItem> items = new List<DomainItem>();

            Terms = cutSpaces(Terms);

            if (!String.IsNullOrEmpty(Terms))
            {
                string[] pairs = Terms.Split(';');
                if (pairs.Length > 1)
                {
                    foreach (string s in pairs)
                    {
                        string temp = cutSpaces(s);

                        if (!String.IsNullOrEmpty(temp))
                        {
                            string[] terms = temp.Split(',');

                            string[] tempArray = new string[terms.Length];
                            for (int i = 0; i < terms.Length; i++)
                            {
                                tempArray[i] = cutSpaces(terms[i]);
                            }
                            if (tempArray.Length > 1)
                            {
                                if (!String.IsNullOrEmpty(tempArray[0]) && !String.IsNullOrEmpty(tempArray[1]))
                                {
                                    items.Add(new DomainItem() { Key = cutSpaces(tempArray[0]), Value = cutSpaces(tempArray[1]) });
                                }
                            }
                            else
                            {
                                if (tempArray.Length > 0)
                                {
                                    if (!String.IsNullOrEmpty(tempArray[0]))
                                    {
                                        items.Add(new DomainItem() { Key = cutSpaces(tempArray[0]), Value = "na" });
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    pairs[0] = cutSpaces(pairs[0]);

                    if (!String.IsNullOrEmpty(pairs[0]))
                    {
                        string[] terms = pairs[0].Split(',');

                        foreach (string s in terms)
                        {
                            string temp = cutSpaces(s);

                            if (!String.IsNullOrEmpty(temp))
                            {
                                items.Add(new DomainItem() { Key = cutSpaces(temp) });
                            }
                        }
                    }
                }
            }
            return items;
        }

        private List<DomainConstraintItemModel> clearEmptyItems(List<DomainConstraintItemModel> list)
        {
            for (int i = 0; i < list.Count;i++ )
            {
                if (list.ElementAt(i).Key == null)
                {
                    list.RemoveAt(i);
                }
            }

            return list;
        }

        #endregion

    }
}
