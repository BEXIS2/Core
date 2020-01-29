using System;
using System.Linq;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using System.Xml;
using BExIS.Xml.Helpers;
using System.Xml.Linq;
using BExIS.Modules.Rpm.UI.Classes;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Modules.Rpm.UI.Models
{
    public class VariablePreview
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool isOptional { get; set; }
        public string Unit { get; set; }
        public string DataType { get; set; }
        public string SystemType { get; set; }

        public VariablePreview()
        {
            this.Id = 0;
            this.Label = "";
            this.Description = "";
            this.isOptional = false;
            this.Unit = "";
            this.DataType = "";
            this.SystemType = "";
        }
    }

    public class StructuredDataStructurePreviewModel
    {
        public List<VariablePreview> VariablePreviews { get; set; }

        public StructuredDataStructurePreviewModel()
        {
            this.VariablePreviews = new List<VariablePreview>();
        }

        public StructuredDataStructurePreviewModel(long DataStructureId)
        {
            this.VariablePreviews = new List<VariablePreview>();
            this.fill(DataStructureId);
        }

        public StructuredDataStructurePreviewModel fill(long dataStructureId)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure datastructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
            VariablePreview variablePreview = new VariablePreview();

            if (datastructure != null)
            {
                foreach (Variable v in DataStructureIO.getOrderedVariables(datastructure))
                {
                    v.Unit = v.Unit ?? new Unit();
                    v.DataAttribute = v.DataAttribute ?? new DataAttribute();
                    v.DataAttribute.DataType = v.DataAttribute.DataType ?? new DataType();

                    variablePreview = new VariablePreview();
                    variablePreview.Id = v.Id;
                    variablePreview.Label = v.Label;
                    variablePreview.Description = v.Description;
                    variablePreview.isOptional = v.IsValueOptional;
                    variablePreview.Unit = v.Unit.Name;
                    variablePreview.DataType = v.DataAttribute.DataType.Name;
                    variablePreview.SystemType = v.DataAttribute.DataType.SystemType;

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
                this.inUse = false;

                DatasetManager datasetManager = null;
                try
                {
                    datasetManager = new DatasetManager();
                    foreach (Dataset d in structuredDataStructure.Datasets)
                    {
                        if (datasetManager.RowAny(d.Id))
                        {
                            this.inUse = true;
                            break;
                        }
                        else
                        {
                            foreach (DatasetVersion dv in d.Versions)
                            {
                                if (datasetManager.GetDatasetVersionEffectiveTupleIds(dv).Any())
                                {
                                    this.inUse = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    datasetManager.Dispose();
                }              

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

                    if (unStructuredDataStructure.Datasets != null && unStructuredDataStructure.Datasets.Any())
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

        private List<DataStructure> getStucturedDataStructures(string searchTerms, DataStructureManager dataStructureManager)
        {
            if (String.IsNullOrEmpty(searchTerms))
                return (dataStructureManager.StructuredDataStructureRepo.Get().Cast<DataStructure>().ToList());
            else
                return (getSearchResult(dataStructureManager.StructuredDataStructureRepo.Get().Cast<DataStructure>().ToList(), searchTerms));
        }

        private List<DataStructure> getUnStucturedDataStructures(string searchTerms, DataStructureManager dataStructureManager)
        {     
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
            DataStructureResultStruct dataStructureResult = new DataStructureResultStruct();

            DataStructureManager dataStructureManager = null;
            try
            {
                dataStructureManager = new DataStructureManager();
                foreach (DataStructure ds in getStucturedDataStructures(saerchTerms, dataStructureManager))
                {
                    dataStructureResult = new DataStructureResultStruct();
                    dataStructureResult.Id = ds.Id;
                    dataStructureResult.Title = ds.Name;
                    dataStructureResult.Description = ds.Description;

                    DatasetManager datasetManager = null;                    
                    try
                    {
                        datasetManager = new DatasetManager();
                        foreach (Dataset d in ds.Datasets)
                        {
                            if (datasetManager.RowAny(d.Id))
                            {
                                dataStructureResult.inUse = true;
                                break;
                            }
                            else
                            {
                                foreach (DatasetVersion dv in d.Versions)
                                {
                                    if (datasetManager.GetDatasetVersionEffectiveTupleIds(dv).Any())
                                    {
                                        dataStructureResult.inUse = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    finally
                    {
                        datasetManager.Dispose();
                    }

                    dataStructureResult.Structured = true;

                    if (previewIds != null && previewIds.Contains(ds.Id))
                        dataStructureResult.Preview = true;

                    this.dataStructureResults.Add(dataStructureResult);
                }

                foreach (DataStructure ds in getUnStucturedDataStructures(saerchTerms, dataStructureManager))
                {
                    dataStructureResult = new DataStructureResultStruct();
                    dataStructureResult.Id = ds.Id;
                    dataStructureResult.Title = ds.Name;
                    dataStructureResult.Description = ds.Description;

                    //if (ds.Datasets.Count > 0)
                    //    dataStructureResult.inUse = true;

                    if (previewIds != null && previewIds.Contains(ds.Id))
                        dataStructureResult.Preview = true;

                    this.dataStructureResults.Add(dataStructureResult);
                }
                return this;
            }
            finally
            {
                dataStructureManager.Dispose();
            }
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

    public class DataStructureCreateModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool isSructured { get; set; }
        public bool inUse { get; set; }
        public bool copy { get; set; }

        public DataStructureCreateModel()
        {
            this.Id = 0;
            this.Name = "";
            this.Description = "";
            this.isSructured = true;
            this.inUse = false;
            this.copy = false;
        }
    }
}

    