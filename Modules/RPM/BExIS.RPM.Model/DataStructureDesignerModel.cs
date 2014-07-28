using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Data;

namespace BExIS.RPM.Model
{
    public class DatasetListElement
    {
        public long Id = 0;
        public string Title = "";

        public DatasetListElement(long Id,string Title)
        {
            this.Id = Id;
            this.Title = Title;
        }

        public DatasetListElement()
        {
            this.Id = 0;
            this.Title = "";
        }
    }

    public class DataStructureDesignerModel
    {
        public DataStructure dataStructure { get; set; }

        public bool structured = true;
        public bool show = true;
        public bool inUse = false;
        public DataTable dataStructureTable { get; set; }
        public List<DataAttribute> dataAttributeList { get; set; }
        public List<Variable> variables { get; set; }
        public List<DatasetListElement> datasets { get; set; }
        public DataStructureTree dataStructureTree { get; set; }
        

        public DataStructureDesignerModel()
        {
            this.dataStructure = new StructuredDataStructure();
            this.dataStructureTable = new DataTable();
            this.dataStructureTree = getDataStructureTree(); 
            structured = true;
            show = true;
            inUse = false;
            variables = new List<Variable>();
            datasets = new List<DatasetListElement>();
        }

        public DataStructureTree getDataStructureTree()
        {
            
            DataStructureTree tree = new DataStructureTree();
            
            return (tree);
        }

        public StructuredDataStructure GetDataStructureByID(long ID)
        {

            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure structuredDataStructure = dsm.StructuredDataStructureRepo.Get(ID);
            this.dataStructure = structuredDataStructure;

            if (this.dataStructure != null)
            {
                XmlDocument doc = (XmlDocument)this.dataStructure.Extra;
                XmlNodeList order;
                if (doc == null)
                {
                    doc = new XmlDocument();
                    doc.LoadXml("<extras><order></order></extras>");
                    order = doc.GetElementsByTagName("order");
                    if (structuredDataStructure.Variables != null)
                    {
                        foreach (Variable v in structuredDataStructure.Variables)
                        {

                            XmlNode variable = doc.CreateNode(XmlNodeType.Element, "variable", null);
                            variable.InnerText = v.Id.ToString();
                            order[0].AppendChild(variable);
                        }
                    }
                    structuredDataStructure.Extra = doc;
                    this.dataStructure = dsm.UpdateStructuredDataStructure(structuredDataStructure);
                }

                this.variables = structuredDataStructure.Variables.ToList();

                order = doc.GetElementsByTagName("order");
                List<Variable> sortedVariables = new List<Variable>();
                if (this.variables != null)
                {
                    foreach (XmlNode x in order[0])
                    {
                        foreach (Variable v in this.variables)
                        {
                            if (v.Id == Convert.ToInt64(x.InnerText))
                                sortedVariables.Add(v);

                        }
                    }
                }
                this.variables = sortedVariables;

                if (this.dataStructure.Datasets == null)
                {
                    inUse = false;
                }
                else
                {
                    if (this.dataStructure.Datasets.Count > 0)
                    {
                        inUse = true;
                        DatasetListElement datasetListElement = new DatasetListElement();
                        DatasetManager dm = new DatasetManager();
                        foreach (Dataset d in structuredDataStructure.Datasets)
                        {
                            if (dm.GetDatasetLatestMetadataVersion(d.Id) != null)
                                datasetListElement = new DatasetListElement(d.Id, dm.GetDatasetLatestMetadataVersion(d.Id).SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText);
                            else
                                datasetListElement = new DatasetListElement(0, "");
                            datasets.Add(datasetListElement);
                        }
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

        public DataStructure GetDataStructureByID(long ID, bool structured)
        {
            this.structured = structured;
            if (structured)
            {
                return this.GetDataStructureByID(ID);
            }
            else
            {
                DataStructureManager dsm = new DataStructureManager();
                UnStructuredDataStructure unStructuredDataStructure = dsm.UnStructuredDataStructureRepo.Get(ID);
                this.dataStructure = unStructuredDataStructure;
               
                if (this.dataStructure != null)
                {
                    this.variables = null;
                    if (this.dataStructure.Datasets == null)
                    {
                        inUse = false;
                    }
                    else
                    {
                        if (this.dataStructure.Datasets.Count > 0)
                        {
                            inUse = true;
                            DatasetListElement datasetListElement = new DatasetListElement();
                            DatasetManager dm = new DatasetManager();
                            foreach (Dataset d in unStructuredDataStructure.Datasets)
                            {
                                datasetListElement = new DatasetListElement(d.Id, dm.GetDatasetLatestMetadataVersion(d.Id).SelectNodes("Metadata/Description/Description/Title/Title")[0].InnerText);
                                datasets.Add(datasetListElement);
                            }
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

        public void BuildDataTable()
        {
            this.dataStructureTable = new DataTable();
                List<string> row = new List<string>();

                for (int i = 0; i <= this.variables.Count; i++)
                {
                    this.dataStructureTable.Columns.Add(new DataColumn());
                }

                DataRow Row = this.dataStructureTable.NewRow();

                List<string> Functions = new List<string>();
                foreach (Variable v in this.variables)
                {
                    Functions.Add(v.Id.ToString() + "?DataStructureId=" + this.dataStructure.Id);
                }
              
                row = Functions;
                row.Insert(0, "Functions");

                Row = this.dataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.dataStructureTable.Rows.Add(Row);

                var Names = from p in this.variables
                            select p.Label;
                row = Names.ToList();
                row.Insert(0,"Name");

                Row = this.dataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.dataStructureTable.Rows.Add(Row);

                var IsValueOptionals = from p in this.variables
                                       select p.IsValueOptional;
                List<bool> tmpIOs = IsValueOptionals.ToList();
                row = tmpIOs.ConvertAll<string>(p => p.ToString());
                row.Insert(0, "Optional");

                Row = this.dataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.dataStructureTable.Rows.Add(Row);

                var VariableIDs = from p in this.variables
                                  select p.Id;
                List<long> tmpVIDs = VariableIDs.ToList();
                row = tmpVIDs.ConvertAll<string>(p => p.ToString());
                row.Insert(0, "VariableID");

                Row = this.dataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.dataStructureTable.Rows.Add(Row);

                var ShortNames = from p in this.variables
                                 select p.DataAttribute.ShortName;
                row = ShortNames.ToList();
                row.Insert(0, "ShortName");

                Row = this.dataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.dataStructureTable.Rows.Add(Row);

                var Descriptions = from p in this.variables
                                   select p.DataAttribute.Description;
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

                var Units = from p in this.variables
                            select p.DataAttribute.Unit;

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

                var DataTypes = from p in this.variables
                               select p.DataAttribute.DataType;
                
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

        public void GetDataAttributeList()
        {
            DataContainerManager DataAttributeManager = new DataContainerManager();
            this.dataAttributeList = DataAttributeManager.DataAttributeRepo.Get().ToList();
        }
    }
}
