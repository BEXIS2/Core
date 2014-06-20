using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.RPM.Model
{
    public class DataStructureDesignerModel
    {
        public DataStructure dataStructure { get; set; }

        public bool structured = true;
        public bool show = true;
        public bool inUse = false;
        public DataTable dataStructureTable { get; set; }
        public IList<DataAttribute> dataAttributeList { get; set; }
        public IList<Variable> variables { get; set; }
        

        public DataStructureDesignerModel()
        {
            this.dataStructure = new StructuredDataStructure();
            this.dataStructureTable = new DataTable();
            structured = true;
            show = true;
            inUse = false;
            variables = null;
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
            this.variables = structuredDataStructure.Variables.ToList();

            if (this.dataStructure.Datasets.Count > 0)
            {
                inUse = true;
            }
            else
            {
                inUse = false;
            }

            if (this.dataStructure != null)
            {
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
                this.variables = null;

                if (this.dataStructure.Datasets.Count > 0)
                {
                    inUse = true;
                }
                else
                {
                    inUse = false;
                }

                if (this.dataStructure != null)
                {
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
