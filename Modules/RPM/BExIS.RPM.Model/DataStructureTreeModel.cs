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
        public String name;
        public long id;
        public bool structured; 

        public DataStructureTreeElement()
        {
            name = " ";
            id = 0;
            structured = true;
        }

    }

    public class DataStructureTreeList
    {
        public String name;
        public List<DataStructureTreeElement> dataStructureTreeElementList;
        

        public DataStructureTreeList()
        {
            name = "";
            dataStructureTreeElementList = new List<DataStructureTreeElement>();
        }

        public DataStructureTreeList(IList<StructuredDataStructure> structuredDatastructureList)
        {
            name = "Structured";
            dataStructureTreeElementList = new List<DataStructureTreeElement>();
            foreach (StructuredDataStructure ds in structuredDatastructureList)
            {
                DataStructureTreeElement treeElement = new DataStructureTreeElement();
                treeElement.name = ds.Name;
                treeElement.id = ds.Id;
                dataStructureTreeElementList.Add(treeElement);
            }
        }

        public DataStructureTreeList(IList<UnStructuredDataStructure> unStructuredDatastructureList)
        {
            name = "Unstructured";
            dataStructureTreeElementList = new List<DataStructureTreeElement>();
            foreach (UnStructuredDataStructure ds in unStructuredDatastructureList)
            {
                DataStructureTreeElement treeElement = new DataStructureTreeElement();
                treeElement.name = ds.Name;
                treeElement.id = ds.Id;
                treeElement.structured = false;
                dataStructureTreeElementList.Add(treeElement);
            }
        }

    }

    public class DataStructureTree
    {
        public List<DataStructureTreeList> dataStructureTreeList;


        public DataStructureTree()
        {
            dataStructureTreeList = new List<DataStructureTreeList>();
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructureTreeList treeList = new DataStructureTreeList(dataStructureManager.StructuredDataStructureRepo.Get());
            dataStructureTreeList.Add(treeList);
            treeList = new DataStructureTreeList(dataStructureManager.UnStructuredDataStructureRepo.Get());
            dataStructureTreeList.Add(treeList);
        }
    }
}
