using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Core;
using Vaiona.Entities;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;

namespace BExIS.RPM.Model
{
    public class DataStructureDesignerModel
    {   
        public StructuredDataStructure dataStructure { get; set; }
        public DataTable DataStructureTable { get; set; }
        public IList<DataAttribute> dataAttributeList { get; set; }
        

        public DataStructureDesignerModel()
        {
            this.dataStructure = new StructuredDataStructure();
            this.DataStructureTable = new DataTable();
        }

        public IList<StructuredDataStructure> GetDataStructureList()
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            return (dataStructureManager.StructuredDataStructureRepo.Get());
        }

        public bool GetDataStructureByID(long ID)
        {
            IList<StructuredDataStructure> DataStructureList = GetDataStructureList();
            if (DataStructureList.Where(p => p.Id.Equals(ID)).Count() > 0)
            {
                this.dataStructure = DataStructureList.Where(p => p.Id.Equals(ID)).First();
                this.BuildDataTable();
                return (true);
            }
            else
            {
                this.dataStructure = new StructuredDataStructure();
                return (false);
            }
        }

        public bool GetDataStructureByID(string ID)
        {
            int dataStructureID = Convert.ToInt32(ID);
            bool datastructure = GetDataStructureByID(dataStructureID);
            return (datastructure);
        }

        public void BuildDataTable()
        { 
                List<string> row = new List<string>();
               
                for (int i = 0; i <= this.dataStructure.Variables.Count; i++)
                {
                    this.DataStructureTable.Columns.Add(new DataColumn());
                }

                DataRow Row = this.DataStructureTable.NewRow();

                var Names = from p in this.dataStructure.Variables
                            select p.DataAttribute.Name;
                row = Names.ToList();
                row.Insert(0,"Name");

                Row = this.DataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.DataStructureTable.Rows.Add(Row);

                var VariableIDs = from p in this.dataStructure.Variables
                                  select p.DataAttribute.Id;
                List<long> tmp = VariableIDs.ToList();
                row = tmp.ConvertAll<string>(p => p.ToString());
                row.Insert(0, "VariableID");

                Row = this.DataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.DataStructureTable.Rows.Add(Row);

                var ShortNames = from p in this.dataStructure.Variables
                                 select p.DataAttribute.ShortName;
                row = ShortNames.ToList();
                row.Insert(0, "ShortName");

                Row = this.DataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.DataStructureTable.Rows.Add(Row);

                var Descriptions = from p in this.dataStructure.Variables
                                   select p.DataAttribute.Description;
                row = Descriptions.ToList();
                row.Insert(0, "Description");

                Row = this.DataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.DataStructureTable.Rows.Add(Row);



                var Classifications = from p in this.dataStructure.Variables
                                      select p.DataAttribute.Classification;

                row = new List<string>();

                foreach(Classifier p in Classifications)
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
                row.Insert(0, "Classification");

                Row = this.DataStructureTable.NewRow();    
                Row.ItemArray = row.ToArray();

                this.DataStructureTable.Rows.Add(Row);

                var Units = from p in this.dataStructure.Variables
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

                Row = this.DataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.DataStructureTable.Rows.Add(Row);

                var DataTypes = from p in this.dataStructure.Variables
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

                Row = this.DataStructureTable.NewRow();
                Row.ItemArray = row.ToArray();

                this.DataStructureTable.Rows.Add(Row);      
        }
    
        public void GetDataAttributeList()
        {
            DataContainerManager DataAttributeManager = new DataContainerManager();
            this.dataAttributeList = DataAttributeManager.DataAttributeRepo.Get();
        }
    }
}
