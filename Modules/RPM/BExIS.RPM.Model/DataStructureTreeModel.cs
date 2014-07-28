using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.RPM.Model
{
    public class DataStructureTreeElement
    {
        public String name = null;
        public long id = 0;
        public bool structured = true;
        public bool inUse = true;

        public DataStructureTreeElement()
        {
            name = " ";
            id = 0;
            structured = true;
        }

    }

    public class DataStructureTreeList
    {
        public String name = null;
        public List<DataStructureTreeElement> dataStructureTreeElementList = null;
        

        public DataStructureTreeList()
        {
            name = "";
            dataStructureTreeElementList = new List<DataStructureTreeElement>();
        }

        public DataStructureTreeList(IList<StructuredDataStructure> structuredDatastructureList)
        {
            name = "Structured";
            dataStructureTreeElementList = new List<DataStructureTreeElement>();
            List<string> names = new List<string>();
            foreach (StructuredDataStructure ds in structuredDatastructureList)
            {
                DataStructureTreeElement treeElement = new DataStructureTreeElement();
                treeElement.name = ds.Name;
                names.Add(ds.Name);
                treeElement.id = ds.Id;

                if (ds.Datasets == null)
                {
                    treeElement.inUse = false;
                }
                else
                {
                    if (ds.Datasets.Count > 0)
                    {
                        treeElement.inUse = true;
                    }
                    else
                    {
                        treeElement.inUse = false;
                    }
                }

                dataStructureTreeElementList.Add(treeElement);
            }
            names.Sort();
            List<DataStructureTreeElement> tempTreeElement = new List<DataStructureTreeElement>();
            foreach (string s in names)
            {
                foreach (DataStructureTreeElement te in this.dataStructureTreeElementList)
                {
                    if (te.name == s)
                        tempTreeElement.Add(te);
                }
            }
            dataStructureTreeElementList = tempTreeElement;
        }

        public DataStructureTreeList(IList<UnStructuredDataStructure> unStructuredDatastructureList)
        {
            name = "Unstructured";
            dataStructureTreeElementList = new List<DataStructureTreeElement>();
            List<string> names = new List<string>();
            foreach (UnStructuredDataStructure ds in unStructuredDatastructureList)
            {
                DataStructureTreeElement treeElement = new DataStructureTreeElement();
                treeElement.name = ds.Name;
                names.Add(ds.Name);
                treeElement.id = ds.Id;
                treeElement.structured = false;

                if (ds.Datasets == null)
                {
                    treeElement.inUse = false;
                }
                else
                {
                    if (ds.Datasets.Count > 0)
                    {
                        treeElement.inUse = true;
                    }
                    else
                    {
                        treeElement.inUse = false;
                    }
                }

                dataStructureTreeElementList.Add(treeElement);
            }
            names.Sort();
            List<DataStructureTreeElement> tempTreeElement = new List<DataStructureTreeElement>();
            foreach (string s in names)
            {
                foreach (DataStructureTreeElement te in this.dataStructureTreeElementList)
                {
                    if (te.name == s)
                        tempTreeElement.Add(te);
                }
            }
            dataStructureTreeElementList = tempTreeElement;
        }

    }

    public class DataStructureTree
    {
        public List<DataStructureTreeList> dataStructureTreeList = null;


        public DataStructureTree()
        {
            dataStructureTreeList = new List<DataStructureTreeList>();
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructureTreeList treeList = new DataStructureTreeList();
            if(dataStructureManager.StructuredDataStructureRepo.Get() != null)
                treeList = new DataStructureTreeList(dataStructureManager.StructuredDataStructureRepo.Get());
            dataStructureTreeList.Add(treeList);
            if (dataStructureManager.UnStructuredDataStructureRepo.Get() !=null)
                treeList = new DataStructureTreeList(dataStructureManager.UnStructuredDataStructureRepo.Get());
            dataStructureTreeList.Add(treeList);
        }
    }
}
