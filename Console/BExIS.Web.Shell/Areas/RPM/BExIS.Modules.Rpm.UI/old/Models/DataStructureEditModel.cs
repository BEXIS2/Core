namespace BExIS.Modules.Rpm.UI.Models
{
    public struct ItemStruct
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
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
        public Dictionary<string, AttributeFilterStruct> AttributeFilterDictionary { get; set; }

        public AttributeFilterModel()
        {
            this.AttributeFilterDictionary = new Dictionary<string, AttributeFilterStruct>();
        }

        public AttributeFilterModel fill()
        {
            this.AttributeFilterDictionary = new Dictionary<string, AttributeFilterStruct>();
            AttributePreviewModel attributePreviewModel = new AttributePreviewModel().fill(false);

            this.AttributeFilterDictionary.Add("Data Type", new AttributeFilterStruct());
            this.AttributeFilterDictionary.Add("Unit", new AttributeFilterStruct());
            this.AttributeFilterDictionary.Add("Dimension", new AttributeFilterStruct());

            string key = "";
            FilterValueStruct value = new FilterValueStruct();

            foreach (AttributePreviewStruct aps in attributePreviewModel.AttributePreviews)
            {

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

                key = aps.Dimension.ToLower().Replace(" ", "");
                value = new FilterValueStruct();

                if (this.AttributeFilterDictionary["Dimension"].Values.ContainsKey(key))
                {
                    this.AttributeFilterDictionary["Dimension"].Values[key].Appearance.Add(aps.Id);
                }
                else
                {
                    value.Name = aps.Dimension;
                    value.Appearance.Add(aps.Id);
                    this.AttributeFilterDictionary["Dimension"].Values.Add(key, value);
                }

                key = aps.Unit.Name.ToLower().Replace(" ", "");
                value = new FilterValueStruct();

                if (this.AttributeFilterDictionary["Unit"].Values.ContainsKey(key))
                {
                    this.AttributeFilterDictionary["Unit"].Values[key].Appearance.Add(aps.Id);
                }
                else
                {
                    value.Name = aps.Unit.Name;
                    value.Appearance.Add(aps.Id);
                    this.AttributeFilterDictionary["Unit"].Values.Add(key, value);
                }
            }
            foreach (KeyValuePair<string, AttributeFilterStruct> kv in this.AttributeFilterDictionary)
            {
                kv.Value.Values = kv.Value.Values.OrderBy(v => v.Value.Name).ToDictionary(v => v.Key, v => v.Value);
            }
            return this;
        }
    }

    public class AttributePreviewStruct
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemStruct Unit;
        public string DataType { get; set; }
        public Dictionary<long, string> Constraints { get; set; }
        public bool inUse { get; set; }
        public string Dimension { get; set; }
        public string DisplayPattern { get; set; }


        public AttributePreviewStruct()
        {
            this.Id = 0;
            this.Name = "";
            this.Description = "";
            this.Unit = new ItemStruct();
            this.DataType = "";
            this.Constraints = new Dictionary<long, string>();
            this.inUse = false;
            this.Dimension = "";
            this.DisplayPattern = "";
        }

        public AttributePreviewStruct fill(long attributeId)
        {
            return this.fill(attributeId, true);
        }

        public AttributePreviewStruct fill(long attributeId, bool getConstraints)
        {
            DataContainerManager dataAttributeManager = null;
            try
            {
                dataAttributeManager = new DataContainerManager();
                DataAttribute DataAttribute = dataAttributeManager.DataAttributeRepo.Get(attributeId);

                return this.fill(DataAttribute, getConstraints);
            }
            finally
            {
                dataAttributeManager.Dispose();
            }
        }

        public AttributePreviewStruct fill(DataAttribute dataAttribute)
        {
            return this.fill(dataAttribute, true);
        }

        public AttributePreviewStruct fill(DataAttribute dataAttribute, bool getConstraints)
        {
            this.Id = dataAttribute.Id;
            this.Name = dataAttribute.Name;
            this.Description = dataAttribute.Description;
            this.Unit.Id = dataAttribute.Unit.Id;
            this.Unit.Name = dataAttribute.Unit.Name;
            this.Unit.Description = dataAttribute.Unit.Abbreviation;
            this.DataType = dataAttribute.DataType.Name;
            this.Dimension = dataAttribute.Unit.Dimension.Name;

            DataTypeDisplayPattern displayPattern = DataTypeDisplayPattern.Materialize(dataAttribute.DataType.Extra);
            if (displayPattern != null)
                this.DisplayPattern = displayPattern.StringPattern;

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

            if (dataAttribute.UsagesAsVariable.Any())
                this.inUse = true;
            else
                this.inUse = false;

            return this;
        }
    }

    public class MissingValueStruct
    {
        public long Id { get; set; }
        public string DisplayName { get; set; }
        public string Placeholder { get; set; }
        public string Description { get; set; }

        public MissingValueStruct()
        {
            this.Id = 0;
            this.DisplayName = "";
            this.Placeholder = "";
            this.Description = "";
        }
    }

    public class VariablePreviewStruct : AttributePreviewStruct
    {
        public bool isOptional { get; set; }
        public List<ItemStruct> convertibleUnits { get; set; }
        public AttributePreviewStruct Attribute { get; set; }
        public List<MissingValueStruct> MissingValues { get; set; }

        public VariablePreviewStruct()
        {
            this.Id = 0;
            this.Name = "";
            this.Description = "";
            this.Unit = new ItemStruct();
            this.convertibleUnits = new List<ItemStruct>();
            this.DataType = "";
            this.Constraints = new Dictionary<long, string>();
            this.Attribute = new AttributePreviewStruct();
            this.inUse = false;
            this.MissingValues = new List<MissingValueStruct>();
            this.DisplayPattern = "";
            this.isOptional = true;
        }

        public new VariablePreviewStruct fill(long attributeId)
        {
            return this.fill(attributeId, true);
        }

        public new VariablePreviewStruct fill(long attributeId, bool getConstraints)
        {
            DataContainerManager dataAttributeManager = null;

            var settings = ModuleManager.GetModuleSettings("Rpm");

            bool optional = true;

            if (settings != null)
            {
                try
                {
                    var optionalDefault = settings.GetEntryValue("optionalDefault");
                    optional = Convert.ToBoolean(optionalDefault);
                }
                catch
                {
                    optional = true;
                }

                try
                {
                    var missingValues = settings.GetList("missingValues");

                    foreach (Item item in missingValues)
                    {
                        this.MissingValues.Add(new MissingValueStruct()
                        {
                            Id = 0,
                            DisplayName = item.GetAttribute("placeholder").Value.ToString(),
                            Description = item.GetAttribute("description").Value.ToString(),
                        });
                    }
                }
                catch
                {
                    this.MissingValues = new List<MissingValueStruct>();
                }
            }

            try
            {
                dataAttributeManager = new DataContainerManager();
                DataAttribute dataAttribute = dataAttributeManager.DataAttributeRepo.Get(attributeId);
                Variable variable = new Variable()
                {
                    Label = dataAttribute.Name,
                    Description = "",
                    Unit = dataAttribute.Unit,
                    DataAttribute = dataAttribute,
                    IsValueOptional = optional
                };
                return this.fill(variable, getConstraints);
            }
            finally
            {
                dataAttributeManager.Dispose();
            }
        }

        public VariablePreviewStruct fill(Variable variable)
        {
            return this.fill(variable, true);
        }

        public VariablePreviewStruct fill(Variable variable, bool getConstraints)
        {
            MissingValueManager missingValueManager = null;

            try
            {
                missingValueManager = new MissingValueManager();
                List<MissingValue> temp = missingValueManager.Repo.Query(mv => mv.Variable.Id.Equals(variable.Id)).ToList();

                variable.Unit = variable.Unit ?? new Unit();
                variable.Unit.Dimension = variable.Unit.Dimension ?? new Dimension();
                variable.DataAttribute = variable.DataAttribute ?? new DataAttribute();
                variable.DataAttribute.DataType = variable.DataAttribute.DataType ?? new DataType();

                this.Id = variable.Id;
                this.Name = variable.Label;
                this.Description = variable.Description;
                this.isOptional = variable.IsValueOptional;
                this.Unit.Id = variable.Unit.Id;
                this.Unit.Name = variable.Unit.Name;
                this.Unit.Description = variable.Unit.Abbreviation;
                this.convertibleUnits = getUnitListByDimenstionAndDataType(variable.Unit.Dimension.Id, variable.DataAttribute.DataType.Id);
                this.DataType = variable.DataAttribute.DataType.Name;

                DataTypeDisplayPattern displayPattern = DataTypeDisplayPattern.Materialize(variable.DataAttribute.DataType.Extra);
                if (displayPattern != null)
                    this.DisplayPattern = displayPattern.StringPattern;

                TypeCode typeCode = TypeCode.String;

                foreach (TypeCode tc in Enum.GetValues(typeof(DataTypeCode)))
                {
                    if (tc.ToString() == variable.DataAttribute.DataType.SystemType)
                    {
                        typeCode = tc;
                        break;
                    }
                }

                if (missingValueManager.getPlaceholder(typeCode, this.Id) != null && temp.Any())
                {
                    foreach (MissingValue mv in temp)
                    {
                        this.MissingValues.Add(new MissingValueStruct()
                        {
                            Id = mv.Id,
                            DisplayName = mv.DisplayName,
                            Description = mv.Description,
                            Placeholder = mv.Placeholder
                        });
                    }
                }
                else if (missingValueManager.getPlaceholder(typeCode, this.Id) == null)
                {
                    this.MissingValues = null;
                }

                if (getConstraints)
                {
                    if (variable.DataAttribute.Constraints != null)
                    {
                        foreach (Constraint c in variable.DataAttribute.Constraints)
                        {
                            c.Materialize();
                            this.Constraints.Add(c.Id, c.FormalDescription);
                        }
                    }
                }

                this.Attribute = Attribute.fill(variable.DataAttribute, false);

                return this;
            }
            finally
            {
                missingValueManager.Dispose();
            }
        }

        public List<ItemStruct> getUnitListByDimenstionAndDataType(long dimensionId, long dataTypeId)
        {
            List<ItemStruct> UnitStructs = new List<ItemStruct>();
            UnitManager unitmanager = null;
            try
            {
                unitmanager = new UnitManager();
                List<Unit> units = new List<Unit>();
                if (unitmanager.DimensionRepo.Get(dimensionId) != null)
                    units = unitmanager.DimensionRepo.Get(dimensionId).Units.ToList();

                foreach (Unit u in units)
                {
                    if (u.Name.ToLower() != "none")
                    {
                        foreach (DataType dt in u.AssociatedDataTypes)
                        {
                            if (dt.Id == dataTypeId)
                            {
                                UnitStructs.Add(new ItemStruct()
                                {
                                    Name = u.Name,
                                    Id = u.Id,
                                    Description = u.Abbreviation
                                });
                                break;
                            }
                        }
                    }
                    else
                    {
                        UnitStructs.Add(new ItemStruct()
                        {
                            Name = u.Name,
                            Id = u.Id,
                            Description = u.Abbreviation
                        });
                    }
                }

                return UnitStructs;
            }
            finally
            {
                unitmanager.Dispose();
            }
        }
    }

    public class AttributeEditStruct : AttributePreviewStruct
    {
        //public ItemStruct DataType { get; set; }
        public List<ItemStruct> Units { get; set; }
        public List<ItemStruct> DataTypes { get; set; }

        public AttributeEditStruct()
        {
            this.Id = 0;
            this.Name = "";
            this.Description = "";
            this.Unit = new ItemStruct();
            //this.DataType = new ItemStruct();
            this.Constraints = new Dictionary<long, string>();
            this.inUse = false;
            this.Units = new List<ItemStruct>();
            this.DataTypes = new List<ItemStruct>();

            UnitManager unitManager = null;

            try
            {
                unitManager = new UnitManager();
                var unitRepo = unitManager.GetUnitOfWork().GetReadOnlyRepository<Unit>();
                foreach (Unit u in unitRepo.Get())
                {
                    this.Units.Add(new ItemStruct()
                    {
                        Name = u.Name,
                        Id = u.Id,
                        Description = u.Description
                    });
                    if (Description == null)
                        Description = "";
                }
            }
            finally
            {
                unitManager.Dispose();
            }
            this.Units = this.Units.OrderBy(u => u.Name).ToList();
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
            DataContainerManager dataAttributeManager = null;
            try
            {
                dataAttributeManager = new DataContainerManager();

                foreach (DataAttribute da in dataAttributeManager.DataAttributeRepo.Get().ToList())
                {
                    this.AttributePreviews.Add(new AttributePreviewStruct().fill(da, getConstraints));
                }
                this.AttributePreviews = this.AttributePreviews.OrderBy(aps => aps.Id).ToList();
                return this;
            }
            finally
            {
                dataAttributeManager.Dispose();
            }
        }
    }

    public class DataStructurePreviewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool inUse { get; set; }
        public List<VariablePreviewStruct> VariablePreviews { get; set; }

        public DataStructurePreviewModel()
        {
            this.Id = 0;
            this.Name = "";
            this.Description = "";
            this.inUse = false;
            this.VariablePreviews = new List<VariablePreviewStruct>();
        }

        public DataStructurePreviewModel fill()
        {
            return this.fill(0);
        }

        public DataStructurePreviewModel fill(long dataStructureId)
        {
            if (dataStructureId > 0)
            {
                DataStructureManager dataStructureManager = null;
                try
                {
                    dataStructureManager = new DataStructureManager();
                    if (dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId) != null)
                    {
                        StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
                        VariablePreviewStruct variablePreview;

                        this.Id = dataStructure.Id;
                        this.Name = dataStructure.Name;
                        this.Description = dataStructure.Description;

                        if (dataStructure.Datasets.Any())
                        {
                            DatasetManager datasetManager = null;
                            try
                            {
                                datasetManager = new DatasetManager();
                                foreach (Dataset d in dataStructure.Datasets)
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
                                            if (datasetManager.GetDatasetVersionEffectiveTuples(dv).Any())
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
                        }

                        foreach (Variable v in DataStructureIO.getOrderedVariables(dataStructure))
                        {
                            variablePreview = new VariablePreviewStruct().fill(v);
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

                        if (dataStructure.Datasets.Any())
                        {
                            this.inUse = true;
                        }
                    }
                    return this;
                }
                finally
                {
                    dataStructureManager.Dispose();
                }
            }
            else
            {
                return new DataStructurePreviewModel();
            }
        }
    }

    public class storeVariableStruct
    {
        public long Id { get; set; }
        public long AttributeId { get; set; }
        public string Lable { get; set; }
        public string Description { get; set; }
        public long UnitId { get; set; }
        public bool isOptional { get; set; }
        public List<MissingValueStruct> MissingValues { get; set; }



        public storeVariableStruct()
        {
            this.Id = 0;
            this.AttributeId = 0;
            this.Lable = "";
            this.Description = "";
            this.UnitId = 0;
            this.isOptional = true;
            this.MissingValues = new List<MissingValueStruct>();
        }
    }

    public class EditUnitStruct
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public string Description { get; set; }
        public long DimensionId { get; set; }
        public string MeasurementSystem { get; set; }

        public List<ItemStruct> DataTypeList;
        public List<ItemStruct> DimensionList;

        public EditUnitStruct()
        {
            this.Id = 0;
            this.Name = "";
            this.Abbreviation = "";
            this.Description = "";
            this.DimensionId = 0;
            this.MeasurementSystem = "";

            this.DataTypeList = new List<ItemStruct>();

            DataTypeManager dataTypeManager = null;
            UnitManager unitManager = null;

            try
            {
                dataTypeManager = new DataTypeManager();

                foreach (DataType dt in dataTypeManager.Repo.Get())
                {
                    this.DataTypeList.Add(new ItemStruct()
                    {
                        Name = dt.Name,
                        Id = dt.Id
                    });
                }

                this.DimensionList = new List<ItemStruct>();

                unitManager = new UnitManager();

                foreach (Dimension dim in unitManager.DimensionRepo.Get())
                {
                    this.DimensionList.Add(new ItemStruct()
                    {
                        Name = dim.Name,
                        Id = dim.Id
                    });
                }
            }
            finally
            {
                dataTypeManager.Dispose();
                unitManager.Dispose();
            }
        }
    }

}
