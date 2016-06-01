using System;
using System.Linq;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class VariablePreviewStruct
    {
        public long Id { get; set; }
        public string Lable { get; set; }
        public string Description { get; set; }
        public bool isOptional { get; set; }
        public string Unit { get; set; }
        public string DataType { get; set; }

        public VariablePreviewStruct()
        {
            this.Id = 0;
            this.Lable = "";
            this.Description = "";
            this.isOptional = false;
            this.Unit = "";
            this.DataType = "";
        }
    }

    public class StructuredDataStructurePreviewModel
    {
        public List<VariablePreviewStruct> VariablePreviews { get; set; }

        public StructuredDataStructurePreviewModel()
        {
            this.VariablePreviews = new List<VariablePreviewStruct>();
        }

        public StructuredDataStructurePreviewModel(long DataStructureId)
        {
            this.VariablePreviews = new List<VariablePreviewStruct>();
            this.fill(DataStructureId);
        }

        public StructuredDataStructurePreviewModel fill(long dataStructureId)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure datastructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
            VariablePreviewStruct variablePreview = new VariablePreviewStruct();

            if (datastructure != null)
            {
                foreach (Variable v in datastructure.Variables)
                {
                    variablePreview = new VariablePreviewStruct();
                    variablePreview.Id = v.Id;
                    variablePreview.Lable = v.Label;
                    variablePreview.Description = v.Description;
                    variablePreview.isOptional = v.IsValueOptional;
                    variablePreview.Unit = v.Unit.Name;
                    variablePreview.DataType = v.DataAttribute.DataType.Name;

                    this.VariablePreviews.Add(variablePreview);
                }
                return this;
            }
            else
            {
                return new StructuredDataStructurePreviewModel();
            } 
        }
    }

    public class DataStructureResultStruct
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool inUse { get; set; }
        public bool Structured { get; set; }
        public bool Preview { get; set; }

        public DataStructureResultStruct()
        {
            this.Id = 0;
            this.Title = "";
            this.Description = "";
            this.inUse = false;
            this.Structured = false;
            this.Preview = false;
        }

        public DataStructureResultStruct(long dataStructureId)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure structuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
            if (structuredDataStructure != null)
            {
                this.Id = structuredDataStructure.Id;
                this.Title = structuredDataStructure.Name;
                this.Description = structuredDataStructure.Description;

                if (structuredDataStructure.Datasets != null && structuredDataStructure.Datasets.Count > 0)
                    this.inUse = true;
                else
                    this.inUse = false;

                this.Structured = true;
                this.Preview = false;
            }
            else 
            {
                UnStructuredDataStructure unStructuredDataStructure = dataStructureManager.UnStructuredDataStructureRepo.Get(dataStructureId);
                if (unStructuredDataStructure != null)
                {
                    this.Id = unStructuredDataStructure.Id;
                    this.Title = unStructuredDataStructure.Name;
                    this.Description = unStructuredDataStructure.Description;

                    if (unStructuredDataStructure.Datasets != null && unStructuredDataStructure.Datasets.Count > 0)
                        this.inUse = true;
                    else
                        this.inUse = false;

                    this.Structured = false;
                    this.Preview = false;
                }
                else
                {
                    new DataStructureResultStruct();
                }
            }
        }
    }

    public class DataStructureResultsModel
    {
        public List<DataStructureResultStruct> dataStructureResults;

        public DataStructureResultsModel()
        {
            dataStructureResults = new List<DataStructureResultStruct>();
        }

        public DataStructureResultsModel(long[] previewIds)
        {
            dataStructureResults = new List<DataStructureResultStruct>();
            this.fill(previewIds);
        }

        public DataStructureResultsModel(long[] previewIds, string saerchTerms)
        {
            dataStructureResults = new List<DataStructureResultStruct>();
            this.fill(previewIds, saerchTerms);
        }

        private List<DataStructure> getStucturedDataStructures(string searchTerms)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            if (String.IsNullOrEmpty(searchTerms))
                return (dataStructureManager.StructuredDataStructureRepo.Get().Cast<DataStructure>().ToList());
            else
                return (getSearchResult(dataStructureManager.StructuredDataStructureRepo.Get().Cast<DataStructure>().ToList(), searchTerms));
        }

        private List<DataStructure> getUnStucturedDataStructures(string searchTerms)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            if (String.IsNullOrEmpty(searchTerms))
                return (dataStructureManager.UnStructuredDataStructureRepo.Get().Cast<DataStructure>().ToList());
            else
                return (getSearchResult(dataStructureManager.UnStructuredDataStructureRepo.Get().Cast<DataStructure>().ToList(), searchTerms));
        }

        private List<DataStructure> getSearchResult(List<DataStructure> datastructures, string searchTerms)
        {
            List<DataStructure> results = new List<DataStructure>();
            string[] terms = searchTerms.Split(' ');
            bool isResult = false;

            foreach (DataStructure ds in datastructures)
            {
                isResult = true;
                for (int i = 0; i < terms.Length; i++)
                {
                    if (String.IsNullOrEmpty(ds.Description))
                        ds.Description = "";

                    if (!ds.Name.ToLower().Contains(terms[i].ToLower()) && !ds.Description.ToLower().Contains(terms[i].ToLower()))
                    {
                        isResult = false;
                        break;
                    }
                }
                if (isResult == true)
                    results.Add(ds);
            }
            return (results);
        }

        public DataStructureResultsModel fill(long[] previewIds, string saerchTerms)
        {
            List<DataStructure> dataStructures = getStucturedDataStructures(saerchTerms);
            DataStructureResultStruct dataStructureResult = new DataStructureResultStruct();


            foreach (DataStructure ds in dataStructures)
            {
                dataStructureResult = new DataStructureResultStruct();
                dataStructureResult.Id = ds.Id;
                dataStructureResult.Title = ds.Name;
                dataStructureResult.Description = ds.Description;

                if (ds.Datasets.Count > 0)
                    dataStructureResult.inUse = true;

                dataStructureResult.Structured = true;

                if (previewIds != null && previewIds.Contains(ds.Id))
                    dataStructureResult.Preview = true;

                this.dataStructureResults.Add(dataStructureResult);
            }

            dataStructures = getUnStucturedDataStructures(saerchTerms);

            foreach (DataStructure ds in dataStructures)
            {
                dataStructureResult = new DataStructureResultStruct();
                dataStructureResult.Id = ds.Id;
                dataStructureResult.Title = ds.Name;
                dataStructureResult.Description = ds.Description;

                if (ds.Datasets.Count > 0)
                    dataStructureResult.inUse = true;

                if (previewIds != null && previewIds.Contains(ds.Id))
                    dataStructureResult.Preview = true;

                this.dataStructureResults.Add(dataStructureResult);
            }
            return this;
        }

        public DataStructureResultsModel fill(long[] previewIds)
        {
            fill(previewIds, null);

            return this;
        }

        public DataStructureResultsModel fill()
        {
            this.fill(null);

            return this;
        }
    }
}

    