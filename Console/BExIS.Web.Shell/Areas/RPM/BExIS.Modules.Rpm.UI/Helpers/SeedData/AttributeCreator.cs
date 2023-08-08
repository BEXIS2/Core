using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.IO.DataType.DisplayPattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Rpm.UI.Helpers.SeedData
{
    public class AttributeCreator
    {
        // create read attributes in bpp
        //public void CreateAttributes(ref DataTable mappedAttributes)
        //{
        //    DataContainerManager attributeManager = null;
        //    DataTypeManager dataTypeManager = null;
        //    UnitManager unitManager = null;
        //    try
        //    {
        //        attributeManager = new DataContainerManager();
        //        dataTypeManager = new DataTypeManager();
        //        unitManager = new UnitManager();

        //        foreach (DataRow mapAttributesRow in mappedAttributes.Rows)
        //        {

        //            DataAttribute attribute = new DataAttribute();

        //            // values of the attribute
        //            attribute.ShortName = mapAttributesRow["ShortName"].ToString();
        //            attribute.Name = mapAttributesRow["Name"].ToString();
        //            attribute.Description = mapAttributesRow["Description"].ToString();
        //            attribute.IsMultiValue = false;
        //            attribute.IsBuiltIn = false;
        //            //attribute.Owner = "BMM";
        //            attribute.Scope = "";
        //            attribute.MeasurementScale = MeasurementScale.Categorial; ////////!!!!!!!!fromMapping??????????????????
        //            attribute.ContainerType = DataContainerType.ReferenceType;
        //            attribute.EntitySelectionPredicate = "";
        //            attribute.DataType = dataTypeManager.Repo.Get(Convert.ToInt64(mapAttributesRow["DataTypeId"]));
        //            attribute.Unit = unitManager.Repo.Get(Convert.ToInt64(mapAttributesRow["UnitId"]));
        //            attribute.Methodology = null;
        //            attribute.Classification = null;
        //            attribute.AggregateFunctions = null;
        //            attribute.GlobalizationInfos = null;
        //            attribute.Constraints = null;
        //            attribute.ExtendedProperties = null;

        //            DataAttribute dataAttribute = new DataAttribute();
        //            DataAttribute existAttribute = attributeManager.DataAttributeRepo.Get(a =>
        //                attribute.Name.Equals(a.Name) &&
        //                attribute.ShortName.Equals(a.ShortName) &&
        //                attribute.Unit.Id.Equals(a.Unit.Id) &&
        //                attribute.DataType.Id.Equals(a.DataType.Id)
        //                ).FirstOrDefault();

        //            // if attribute not exists (name, shortName) then create
        //            if (existAttribute == null)
        //            {
        //                dataAttribute = attributeManager.CreateDataAttribute(
        //                    attribute.ShortName,
        //                    attribute.Name,
        //                    attribute.Description,
        //                    attribute.IsMultiValue,
        //                    attribute.IsBuiltIn,
        //                    attribute.Scope,
        //                    attribute.MeasurementScale,
        //                    attribute.ContainerType,
        //                    attribute.EntitySelectionPredicate,
        //                    attribute.DataType,
        //                    attribute.Unit,
        //                    attribute.Methodology,
        //                    attribute.Classification,
        //                    attribute.AggregateFunctions,
        //                    attribute.GlobalizationInfos,
        //                    attribute.Constraints,
        //                    attribute.ExtendedProperties
        //                    );
        //            }
        //            else
        //            {
        //                dataAttribute = existAttribute;
        //            }

        //            // add attributeId to the mappedAttributes Table
        //            mapAttributesRow["AttributeId"] = dataAttribute.Id;
        //        }
        //    }
        //    finally
        //    {
        //        attributeManager.Dispose();
        //        dataTypeManager.Dispose();
        //        unitManager.Dispose();
        //    }
        //}


        // create read units in bpp
        public void CreateUnits(ref DataTable mappedUnits)
        {
            UnitManager unitManager = null;
            DataTypeManager dataTypeManger = null;
            try
            {
                // Javad: The whole loop can be improved!
                var dimentionRepo = unitManager.GetUnitOfWork().GetReadOnlyRepository<Dimension>();
                foreach (DataRow mapUnitsRow in mappedUnits.Rows)
                {
                    unitManager = new UnitManager();
                    dataTypeManger = new DataTypeManager();
                    Unit unit = new Unit(); // Javad: this can not be out of the loop. in that case, it keeps its link to the session objects and causes the previous unit to be overriden as well as creating a new one!!
                                            // values of the unit
                    unit.Name = mapUnitsRow["Name"].ToString();
                    unit.Abbreviation = mapUnitsRow["Abbreviation"].ToString();
                    unit.Description = mapUnitsRow["Description"].ToString();

                    if (unit.Description.Length > 255)
                        unit.Description = unit.Description.Substring(0, 255);

                    unit.Dimension = dimentionRepo.Get(Convert.ToInt64(mapUnitsRow["DimensionId"]));

                    // find measurement system
                    foreach (MeasurementSystem msCheck in Enum.GetValues(typeof(MeasurementSystem)))
                    {
                        if (msCheck.ToString().Equals(mapUnitsRow["MeasurementSystem"].ToString()))
                        {
                            unit.MeasurementSystem = msCheck;
                        }
                    }

                    // set data type to created unit or add data type to existing unit
                    List<string> Types = mapUnitsRow["DataTypes"].ToString().Split(',').Distinct().ToList();

                    // get existing unit or create
                    Unit existU = unitManager.Repo.Get(u => u.Name.ToLower().Equals(unit.Name.ToLower())).FirstOrDefault();
                    if (existU == null)
                    {
                        unit = unitManager.Create(unit.Name, unit.Abbreviation, unit.Description, unit.Dimension, unit.MeasurementSystem);
                        addDataTypes(unit.Id, Types, unitManager, dataTypeManger);
                    }
                    else
                    {
                        addDataTypes(existU.Id, Types, unitManager, dataTypeManger);
                    }
                    // add unit-ID to the mappedUnits Table
                    mapUnitsRow["UnitId"] = unit.Id;
                }
            }
            finally
            {
                unitManager.Dispose();
                dataTypeManger.Dispose();
            }
        }

        private void addDataTypes(long unitId, List<string> datatypeNames, UnitManager unitManager, DataTypeManager dataTypeManger)
        {
            if (unitId <= 0 || datatypeNames == null || datatypeNames.Count <= 0)
                return;
            var unitRepo = unitManager.GetUnitOfWork().GetReadOnlyRepository<Unit>();
            var dataTypeRepo = dataTypeManger.GetUnitOfWork().GetReadOnlyRepository<DataType>();

            Unit unit = unitRepo.Get(unitId);
            // add bpp-dataTypes to the unit
            if (unit == null)
                return;
            DataType dt = new DataType();
            foreach (string type in datatypeNames)
            {
                dt = dataTypeRepo.Get().Where(d => d.Name.ToLower().Equals(type.ToLower())).FirstOrDefault();
                if (dt != null && !(unit.AssociatedDataTypes.Contains(dt)))
                    unit.AssociatedDataTypes.Add(dt);
            }
            unitManager.Update(unit);
        }

        // create dimensions in bpp
        public void CreateDimensions(ref DataTable mappedDimensions)
        {
            UnitManager unitManager = null;
            try
            {
                unitManager = new UnitManager();

                foreach (DataRow mappedDimension in mappedDimensions.Rows)
                {
                    string dimensionName = mappedDimension["Name"].ToString();
                    string dimensionDescription = mappedDimension["Description"].ToString();
                    string dimensionSpecification = mappedDimension["Syntax"].ToString();

                    Dimension dimension = unitManager.DimensionRepo.Get().Where(d => d.Name.Equals(dimensionName)).FirstOrDefault();

                    if (dimension == null)
                    {
                        dimension = unitManager.Create(dimensionName, dimensionDescription, dimensionSpecification);
                    }

                    mappedDimension["DimensionId"] = dimension.Id;
                }
            }
            finally
            {
                unitManager.Dispose();
            }
        }


        // create read data types in bpp
        public void CreateDataTypes(ref DataTable mappedDataTypes)
        {
            DataTypeManager dataTypeManager = null;
            try
            {
                dataTypeManager = new DataTypeManager();

                foreach (DataRow mappedDataType in mappedDataTypes.Rows)
                {
                    string dtName = mappedDataType["Name"].ToString();
                    string dtDescription = mappedDataType["Description"].ToString();
                    DataTypeDisplayPattern dtDisplayPettern = new DataTypeDisplayPattern();
                    TypeCode dtSystemType = new TypeCode();
                    foreach (TypeCode type in Enum.GetValues(typeof(TypeCode)))
                    {
                        if (type.ToString().Equals(mappedDataType["SystemType"].ToString()))
                        {
                            dtSystemType = type;
                        }
                    }

                    if (dtSystemType == TypeCode.DateTime)
                    {
                        if (mappedDataType["DisplayPattern"] != null && mappedDataType["DisplayPattern"].ToString() != "")
                        {
                            dtDisplayPettern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.Equals(DataTypeCode.DateTime) && p.Name.Equals(mappedDataType["DisplayPattern"].ToString())).FirstOrDefault();
                        }
                        else
                        {
                            dtDisplayPettern = DataTypeDisplayPattern.Pattern.Where(p => p.Name.Equals("DateTimeIso")).FirstOrDefault();
                        }
                    }

                    DataType dataType = new DataType();
                    // get existing dataTypes
                    DataType existDT = dataTypeManager.Repo.Get().Where(d =>
                        d.Name.Equals(dtName) &&
                        d.SystemType.ToString().Equals(mappedDataType["SystemType"].ToString())
                        ).FirstOrDefault();
                    // return ID of existing dataType or create dataType
                    if (existDT == null && dtSystemType != null)
                    {
                        dataType = dataTypeManager.Create(dtName, dtDescription, dtSystemType);

                        XmlDocument xmlDoc = new XmlDocument();
                        XmlNode xmlNode;
                        xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "Extra", null);
                        xmlDoc.AppendChild(xmlNode);
                        xmlNode = xmlDoc.CreateNode(XmlNodeType.Element, "DisplayPattern", null);
                        xmlNode.InnerXml = DataTypeDisplayPattern.Dematerialize(dtDisplayPettern).InnerXml;
                        xmlDoc.DocumentElement.AppendChild(xmlNode);
                        dataType.Extra = xmlDoc;

                        dataTypeManager.Update(dataType);
                    }
                    else
                    {
                        dataType = existDT;
                    }

                    mappedDataType["DataTypesId"] = dataType.Id;
                }
            }
            finally
            {
                dataTypeManager.Dispose();
            }
        }
    }
}
