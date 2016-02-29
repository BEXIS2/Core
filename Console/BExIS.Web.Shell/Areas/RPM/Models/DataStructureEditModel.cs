using System;
using System.Linq;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class FilterValueStruct
    {
        public string Name { get; set; }
        public List<long> Appearance { get; set; }

        public FilterValueStruct()
        {
            this.Name = "";
            this.Appearance = new List<long>();
        }
    } 
    public class AttributeFilterStruct
    {
        public Dictionary<string, FilterValueStruct> Values { get; set; }

        public AttributeFilterStruct()
        {
            this.Values = new Dictionary<string, FilterValueStruct>();
        }
    }

    public class AttributeFilterModel
    {
        public Dictionary<string,AttributeFilterStruct> AttributeFilterDictionary { get; set; }

        public AttributeFilterModel()
        {
            this.AttributeFilterDictionary = new Dictionary<string, AttributeFilterStruct>();
        }

        public AttributeFilterModel fill()
        {
            this.AttributeFilterDictionary = new Dictionary<string, AttributeFilterStruct>();
            AttributePreviewModel attributePreviewModel = new AttributePreviewModel().fill(false);

            this.AttributeFilterDictionary.Add("Unit", new AttributeFilterStruct());
            this.AttributeFilterDictionary.Add("Data Type", new AttributeFilterStruct());

            string key = "";
            FilterValueStruct value = new FilterValueStruct();

            foreach (AttributePreviewStruct aps in attributePreviewModel.AttributePreviews)
            {
                key = aps.Unit.ToLower().Replace(" ", "");
                value = new FilterValueStruct();

                if (this.AttributeFilterDictionary["Unit"].Values.ContainsKey(key))
                {
                    this.AttributeFilterDictionary["Unit"].Values[key].Appearance.Add(aps.Id);
                }
                else
                {
                    value.Name = aps.Unit;
                    value.Appearance.Add(aps.Id);
                    this.AttributeFilterDictionary["Unit"].Values.Add(key, value);
                }

                key = aps.DataType.ToLower().Replace(" ", "");
                value = new FilterValueStruct();

                if (this.AttributeFilterDictionary["Data Type"].Values.ContainsKey(key))
                {
                    this.AttributeFilterDictionary["Data Type"].Values[key].Appearance.Add(aps.Id);
                }
                else
                {
                    value.Name = aps.DataType;
                    value.Appearance.Add(aps.Id);
                    this.AttributeFilterDictionary["Data Type"].Values.Add(key, value);
                }
            }
            return this;
        }
    }

    public class AttributePreviewStruct
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string DataType { get; set; }
        public List<string> Constraints { get; set; }


        public AttributePreviewStruct()
        {
            this.Id = 0;
            this.Name = "";
            this.Description = "";
            this.Unit = "";
            this.DataType = "";
            this.Constraints = new List<string>();
        }
    }

    public class AttributePreviewModel
    {
        public List<AttributePreviewStruct> AttributePreviews { get; set; }

        public AttributePreviewModel()
        {
            this.AttributePreviews = new List<AttributePreviewStruct>();
        }

        public AttributePreviewModel fill()
        {
            return this.fill(true);
        }

        public AttributePreviewModel fill(bool getConstraints)
        {
            this.AttributePreviews = new List<AttributePreviewStruct>();
            DataContainerManager dataAttributeManager = new DataContainerManager();
            AttributePreviewStruct attributePreview;

            foreach (DataAttribute da in dataAttributeManager.DataAttributeRepo.Get().ToList())
            {
                attributePreview = new AttributePreviewStruct();
                attributePreview.Id = da.Id;
                attributePreview.Name = da.Name;
                attributePreview.Description = da.Description;
                attributePreview.Unit = da.Unit.Name;
                attributePreview.DataType = da.DataType.Name;

                if (getConstraints)
                {
                    if (da.Constraints != null)
                    {
                        foreach (Constraint c in da.Constraints)
                        {
                            c.Materialize();
                            attributePreview.Constraints.Add(c.FormalDescription);
                        }
                    }
                }
                this.AttributePreviews.Add(attributePreview);
            }
            return this;
        }
    }

    public class DataStructurePreviewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool inUse { get; set; }
        public List<AttributePreviewStruct> VariablePreviews { get; set; }

        public DataStructurePreviewModel()
        {
            this.Id = 0;
            this.Name = "";
            this.Description = "";
            this.inUse = false;
            this.VariablePreviews = new List<AttributePreviewStruct>();
        }

        public DataStructurePreviewModel fill()
        {
            return this.fill(0);
        }

        public DataStructurePreviewModel fill(long dataStructureId)
        {
            if (dataStructureId > 0)
            {
                DataStructureManager dataStructureManager = new DataStructureManager();
                if (dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId) != null)
                {
                    StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
                    AttributePreviewStruct variablePreview;

                    this.Id = dataStructure.Id;
                    this.Name = dataStructure.Name;
                    this.Description = dataStructure.Description;

                    if(dataStructure.Datasets.Count > 0)
                    {
                        this.inUse = true;
                    }

                    foreach (Variable v in dataStructure.Variables)
                    {
                        variablePreview = new AttributePreviewStruct();
                        variablePreview.Id = v.Id;
                        variablePreview.Name = v.Label;
                        variablePreview.Description = v.Description;
                        variablePreview.Unit = v.Unit.Name;
                        variablePreview.DataType = v.DataAttribute.DataType.Name;

                        if (v.DataAttribute.Constraints != null)
                        {
                            foreach (Constraint c in v.DataAttribute.Constraints)
                            {
                                c.Materialize();
                                variablePreview.Constraints.Add(c.FormalDescription);
                            }
                        }
                        this.VariablePreviews.Add(variablePreview);
                    }
                }
                else if (dataStructureManager.UnStructuredDataStructureRepo.Get(dataStructureId) != null)
                {
                    UnStructuredDataStructure dataStructure = dataStructureManager.UnStructuredDataStructureRepo.Get(dataStructureId);

                    this.Id = dataStructure.Id;
                    this.Name = dataStructure.Name;
                    this.Description = dataStructure.Description;
                    this.VariablePreviews = null;

                    if(dataStructure.Datasets.Count > 0)
                    {
                        this.inUse = true;
                    }
                }
                return this;
            }
            else
            {
                return new DataStructurePreviewModel();
            }
        }
    }
}
    