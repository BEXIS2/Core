using System;
using System.Linq;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public struct UnitStruct
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }
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
                key = aps.Unit.Name.ToLower().Replace(" ", "");
                value = new FilterValueStruct();

                if (this.AttributeFilterDictionary["Unit"].Values.ContainsKey(key))
                {
                    this.AttributeFilterDictionary["Unit"].Values[key].Appearance.Add(aps.AttributeId);
                }
                else
                {
                    value.Name = aps.Unit.Name;
                    value.Appearance.Add(aps.AttributeId);
                    this.AttributeFilterDictionary["Unit"].Values.Add(key, value);
                }

                key = aps.DataType.ToLower().Replace(" ", "");
                value = new FilterValueStruct();

                if (this.AttributeFilterDictionary["Data Type"].Values.ContainsKey(key))
                {
                    this.AttributeFilterDictionary["Data Type"].Values[key].Appearance.Add(aps.AttributeId);
                }
                else
                {
                    value.Name = aps.DataType;
                    value.Appearance.Add(aps.AttributeId);
                    this.AttributeFilterDictionary["Data Type"].Values.Add(key, value);
                }
            }
            return this;
        }
    }

    public class AttributePreviewStruct
    {
        public long AttributeId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public UnitStruct Unit;
        public List<UnitStruct> convertibleUnits;
        public string DataType { get; set; }
        public bool isVariable { get; set; }
        public long VariableId { get; set; }
        public Dictionary<long,string> Constraints { get; set; }


        public AttributePreviewStruct()
        {
            this.AttributeId = 0;
            this.Name = "";
            this.Description = "";
            this.Unit = new UnitStruct();
            this.convertibleUnits = new List<UnitStruct>();
            this.DataType = "";
            this.isVariable = false;
            this.VariableId = 0;
            this.Constraints = new Dictionary<long, string>();
        }

        public AttributePreviewStruct fill(long attributeId)
        {
            return this.fill(attributeId, true);
        }

        public AttributePreviewStruct fill(long attributeId, bool getConstraints)
        {
            DataContainerManager dataAttributeManager = new DataContainerManager();
            DataAttribute DataAttribute = dataAttributeManager.DataAttributeRepo.Get(attributeId);
            
            return this.fill(DataAttribute);
        }

        public AttributePreviewStruct fill(DataAttribute dataAttribute)
        {
            return this.fill(dataAttribute, true);
        }

        public AttributePreviewStruct fill(DataAttribute dataAttribute, bool getConstraints)
        {
            this.AttributeId = dataAttribute.Id;
            this.Name = dataAttribute.Name;
            this.Description = dataAttribute.Description;
            this.Unit.Id = dataAttribute.Unit.Id;
            this.Unit.Name = dataAttribute.Unit.Name;
            this.convertibleUnits = getUnitListByDimenstionAndDataType(dataAttribute.Unit.Dimension.Id, dataAttribute.DataType.Id);
            this.DataType = dataAttribute.DataType.Name;
            this.isVariable = false;
            this.VariableId = 0;

            if (getConstraints)
            {
                if (dataAttribute.Constraints != null)
                {
                    foreach (Constraint c in dataAttribute.Constraints)
                    {
                        c.Materialize();
                        this.Constraints.Add(c.Id, c.FormalDescription);
                    }
                }
            }
            return this;
        }

        public List<UnitStruct> getUnitListByDimenstionAndDataType(long dimensionId, long dataTypeId)
        {
            List<UnitStruct> UnitStructs = new List<UnitStruct>();
            UnitManager unitmanager = new UnitManager();
            List<Unit> units = unitmanager.DimensionRepo.Get(dimensionId).Units.ToList();
            UnitStruct tempUnitStruct = new UnitStruct();
            foreach (Unit u in units)
            {
                if (u.Name.ToLower() != "none")
                {
                    foreach (DataType dt in u.AssociatedDataTypes)
                    {
                        if (dt.Id == dataTypeId)
                        {
                            tempUnitStruct.Id = u.Id;
                            tempUnitStruct.Name = u.Name;
                            UnitStructs.Add(tempUnitStruct);
                            break;
                        }
                    }
                }
                else
                {
                    tempUnitStruct.Id = u.Id;
                    tempUnitStruct.Name = u.Name;
                    UnitStructs.Add(tempUnitStruct);
                }
            }

            return UnitStructs;
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

            foreach (DataAttribute da in dataAttributeManager.DataAttributeRepo.Get().ToList())
            {                
                this.AttributePreviews.Add(new AttributePreviewStruct().fill(da, getConstraints));
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
                        variablePreview = new AttributePreviewStruct().fill(v.DataAttribute);
                        variablePreview.Description = v.Description;
                        variablePreview.Name = v.Label;
                        variablePreview.isVariable = true;
                        variablePreview.VariableId = v.Id;

                        if (v.DataAttribute.Constraints != null)
                        {
                            foreach (Constraint c in v.DataAttribute.Constraints)
                            {
                                c.Materialize();
                                variablePreview.Constraints.Add(c.Id,c.FormalDescription);
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
    