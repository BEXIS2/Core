using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using XmlNodeType = System.Xml.XmlNodeType;

/// <summary>
///
/// </summary>        
namespace BExIS.Modules.Rpm.UI.Models
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>

    public struct VariableStruct
    {
        public Variable variable;
        public List<ItemStruct> unitStructs;
        public List<DomainConstraint> domainConstraints;
        public List<PatternConstraint> patternConstraints;
        public List<RangeConstraint> rangeConstraints;
    }

    public class DatasetListElement
    {
        public long Id = 0;
        public string Title = "";

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="Id"></param>
        /// <param name="Title"></param>
        public DatasetListElement(long Id, string Title)
        {
            this.Id = Id;
            this.Title = Title;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public DatasetListElement()
        {
            this.Id = 0;
            this.Title = "";
        }
    }


    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class DataStructureDesignerModel
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public DataStructure dataStructure { get; set; }

        public bool structured = true;
        public bool show = true;
        public bool inUse = false;

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public DataTable dataStructureTable { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public List<DataAttribute> dataAttributeList { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>

        public List<VariableStruct> variableStructs { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public List<DatasetListElement> datasets { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public DataStructureTree dataStructureTree { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public DataStructureDesignerModel()
        {
            this.dataStructure = new StructuredDataStructure();
            this.dataStructureTable = new DataTable();
            this.dataStructureTree = getDataStructureTree();
            structured = true;
            show = true;
            inUse = false;
            variableStructs = new List<VariableStruct>();
            datasets = new List<DatasetListElement>();
        }


        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        /// <returns></returns>
        public DataStructureTree getDataStructureTree()
        {

            DataStructureTree tree = new DataStructureTree();

            return (tree);
        }

        public List<VariableStruct> getOrderedVariableStructs(StructuredDataStructure structuredDataStructure)
        {
            List<VariableStruct> variableStructs = new List<VariableStruct>();
            List<Variable> variables = getOrderedVariables(structuredDataStructure);
            DataContainerManager dataAttributeManager = null;
            VariableStruct temp = new VariableStruct();
            List<BExIS.Dlm.Entities.DataStructure.Constraint> tempconstraints;
            UnitDimenstionModel unitDimenstionModel = new UnitDimenstionModel();
            foreach (Variable v in variables)
            {
                try
                {
                    unitDimenstionModel = new UnitDimenstionModel();
                    temp.variable = v;
                    temp.unitStructs = unitDimenstionModel.getUnitListByDimenstionAndDataType(v.DataAttribute.Unit.Dimension.Id, v.DataAttribute.DataType.Id);
                    dataAttributeManager = new DataContainerManager();
                    tempconstraints = dataAttributeManager.DataAttributeRepo.Get(v.DataAttribute.Id).Constraints.ToList();
                    temp.rangeConstraints = new List<RangeConstraint>();
                    temp.domainConstraints = new List<DomainConstraint>();
                    temp.patternConstraints = new List<PatternConstraint>();
                    foreach (BExIS.Dlm.Entities.DataStructure.Constraint c in tempconstraints)
                    {
                        if (c is DomainConstraint)
                        {
                            DomainConstraint tempDomainConstraint = (DomainConstraint)c;
                            tempDomainConstraint.Materialize();
                            temp.domainConstraints.Add(tempDomainConstraint);
                        }
                        if (c is PatternConstraint)
                            temp.patternConstraints.Add((PatternConstraint)c);
                        if (c is RangeConstraint)
                            temp.rangeConstraints.Add((RangeConstraint)c);

                    }
                    variableStructs.Add(temp);
                }
                finally
                {
                    dataAttributeManager.Dispose();
                }
            }
            return variableStructs;
        }
        public List<Variable> getOrderedVariables(StructuredDataStructure structuredDataStructure)
        {
            using (DataStructureManager dsm = new DataStructureManager())
            {
                XmlDocument doc = (XmlDocument)structuredDataStructure.Extra;
                XmlNode order;

                if (doc == null)
                {
                    doc = new XmlDocument();
                    XmlNode root = doc.CreateNode(XmlNodeType.Element, "extra", null);
                    doc.AppendChild(root);
                }
                if (doc.GetElementsByTagName("order").Count == 0)
                {

                    if (structuredDataStructure.Variables.Count > 0)
                    {
                        order = doc.CreateNode(XmlNodeType.Element, "order", null);

                        foreach (Variable v in structuredDataStructure.Variables)
                        {

                            XmlNode variable = doc.CreateNode(XmlNodeType.Element, "variable", null);
                            variable.InnerText = v.Id.ToString();
                            order.AppendChild(variable);
                        }

                        doc.FirstChild.AppendChild(order);
                        structuredDataStructure.Extra = doc;
                        dsm.UpdateStructuredDataStructure(structuredDataStructure);
                    }
                }

                order = doc.GetElementsByTagName("order")[0];
                List<Variable> orderedVariables = new List<Variable>();
                if (structuredDataStructure.Variables.Count != 0)
                {
                    foreach (XmlNode x in order)
                    {
                        foreach (Variable v in structuredDataStructure.Variables)
                        {
                            if (v.Id == Convert.ToInt64(x.InnerText))
                                orderedVariables.Add(v);

                        }
                    }
                }
                return orderedVariables;
            }
        
        }

        public StructuredDataStructure GetDataStructureByID(long ID)
        {
            using (DataStructureManager dsm = new DataStructureManager())
            {
                StructuredDataStructure structuredDataStructure = dsm.StructuredDataStructureRepo.Get(ID);
                this.dataStructure = structuredDataStructure;

                if (this.dataStructure != null)
                {
                    this.variableStructs = getOrderedVariableStructs(structuredDataStructure);

                    if (this.dataStructure.Datasets == null)
                    {
                        inUse = false;
                    }
                    else
                    {
                        if (this.dataStructure.Datasets.Count > 0)
                        {
                            inUse = true;
                        }
                        else
                        {
                            inUse = false;
                        }
                    }
                    this.BuildDataTable();
                    return (structuredDataStructure);
                }
                else
                {
                    this.dataStructure = new StructuredDataStructure();
                    return (structuredDataStructure);
                }

            }
        }

        public void fillDatasetList()
        {
            using (DatasetManager dm = new DatasetManager())
            {

                DatasetListElement datasetListElement = new DatasetListElement();
                datasets = new List<DatasetListElement>();

                XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();


                foreach (var item in dm.GetDatasetLatestVersions(dataStructure.Id, true))
                {
                    datasetListElement = new DatasetListElement(item.Key, item.Value.Title);
                    datasets.Add(datasetListElement);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="ID"></param>
        /// <param name="structured"></param>
        /// <returns></returns>
        public DataStructure GetDataStructureByID(long ID, bool structured)
        {
            this.structured = structured;
            if (structured)
            {
                return this.GetDataStructureByID(ID);
            }
            else
            {
                using (DataStructureManager dsm = new DataStructureManager())
                {

                    UnStructuredDataStructure unStructuredDataStructure = dsm.UnStructuredDataStructureRepo.Get(ID);
                    this.dataStructure = unStructuredDataStructure;

                    if (this.dataStructure != null)
                    {
                        this.variableStructs = null;
                        if (this.dataStructure.Datasets == null)
                        {
                            inUse = false;
                        }
                        else
                        {
                            if (this.dataStructure.Datasets.Count > 0)
                            {
                                inUse = true;
                                //DatasetListElement datasetListElement = new DatasetListElement();
                                //DatasetManager dm = new DatasetManager();
                                //foreach (Dataset d in unStructuredDataStructure.Datasets)
                                //{
                                //    datasetListElement = new DatasetListElement(d.Id, XmlDatasetHelper.GetInformation(d,AttributeNames.title));
                                //    datasets.Add(datasetListElement);
                                //}
                            }
                            else
                            {
                                inUse = false;
                            }
                        }
                        return (unStructuredDataStructure);
                    }
                    else
                    {
                        this.dataStructure = new StructuredDataStructure();
                        return (unStructuredDataStructure);
                    }
                }
                
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public void BuildDataTable()
        {
            this.dataStructureTable = new DataTable();
            List<string> row = new List<string>();

            for (int i = 0; i <= this.variableStructs.Count; i++)
            {
                this.dataStructureTable.Columns.Add(new DataColumn());
            }

            DataRow Row = this.dataStructureTable.NewRow();

            List<string> Functions = new List<string>();
            foreach (VariableStruct v in this.variableStructs)
            {
                Functions.Add(v.variable.Id.ToString() + "?DataStructureId=" + this.dataStructure.Id);
            }

            row = Functions;
            row.Insert(0, "Functions");

            Row = this.dataStructureTable.NewRow();
            Row.ItemArray = row.ToArray();

            this.dataStructureTable.Rows.Add(Row);

            var Names = from p in this.variableStructs
                        select p.variable.Label;
            row = Names.ToList();
            row.Insert(0, "Name");

            Row = this.dataStructureTable.NewRow();
            Row.ItemArray = row.ToArray();

            this.dataStructureTable.Rows.Add(Row);

            var IsValueOptionals = from p in this.variableStructs
                                   select p.variable.IsValueOptional;
            List<bool> tmpIOs = IsValueOptionals.ToList();
            row = tmpIOs.ConvertAll<string>(p => p.ToString());
            row.Insert(0, "Optional");

            Row = this.dataStructureTable.NewRow();
            Row.ItemArray = row.ToArray();

            this.dataStructureTable.Rows.Add(Row);

            var VariableIDs = from p in this.variableStructs
                              select p.variable.Id;
            List<long> tmpVIDs = VariableIDs.ToList();
            row = tmpVIDs.ConvertAll<string>(p => p.ToString());
            row.Insert(0, "Variable Id");

            Row = this.dataStructureTable.NewRow();
            Row.ItemArray = row.ToArray();

            this.dataStructureTable.Rows.Add(Row);

            var ShortNames = from p in this.variableStructs
                             select p.variable.DataAttribute.ShortName;
            row = ShortNames.ToList();
            row.Insert(0, "Short Name");

            Row = this.dataStructureTable.NewRow();
            Row.ItemArray = row.ToArray();

            this.dataStructureTable.Rows.Add(Row);

            var Descriptions = from p in this.variableStructs
                               select getDescription(p.variable);
            row = Descriptions.ToList();
            row.Insert(0, "Description");

            Row = this.dataStructureTable.NewRow();
            Row.ItemArray = row.ToArray();

            this.dataStructureTable.Rows.Add(Row);

            //var Classifications = from p in this.dataStructure.Variables
            //                      select p.DataAttribute.Classification;

            //row = new List<string>();

            //foreach(Classifier p in Classifications)
            //{
            //    if (p == null)
            //    {
            //        row.Add("");
            //    }
            //    else
            //    {
            //        row.Add(p.Name);
            //    }
            //}
            //row.Insert(0, "Classification");

            //Row = this.DataStructureTable.NewRow();    
            //Row.ItemArray = row.ToArray();

            //this.DataStructureTable.Rows.Add(Row);

            var Units = from p in this.variableStructs
                        select p.variable.Unit;

            row = new List<string>();

            foreach (Unit p in Units)
            {
                if (p == null)
                {
                    row.Add("");
                }
                else
                {
                    row.Add(p.Name);
                }
            }
            row.Insert(0, "Unit");

            Row = this.dataStructureTable.NewRow();
            Row.ItemArray = row.ToArray();

            this.dataStructureTable.Rows.Add(Row);

            var DataTypes = from p in this.variableStructs
                            select p.variable.DataAttribute.DataType;

            row = new List<string>();

            foreach (DataType p in DataTypes)
            {
                if (p == null)
                {
                    row.Add("");
                }
                else
                {
                    row.Add(p.Name);
                }
            }
            row.Insert(0, "Data Type");

            Row = this.dataStructureTable.NewRow();
            Row.ItemArray = row.ToArray();

            this.dataStructureTable.Rows.Add(Row);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public void GetDataAttributeList()
        {
            DataContainerManager DataAttributeManager = null;
            try
            {
                DataAttributeManager = new DataContainerManager();
                this.dataAttributeList = DataAttributeManager.DataAttributeRepo.Get().ToList();
            }
            finally
            {
                DataAttributeManager.Dispose();
            }
        }

        #region helper

        private string getDescription(Variable v)
        {
            if (v.Description != null && v.Description != "")
                return v.Description;
            else
                return v.DataAttribute.Description;
        }

        #endregion
    }
}
