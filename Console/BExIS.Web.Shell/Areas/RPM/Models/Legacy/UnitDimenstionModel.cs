using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class UnitDimenstionModel
    {
        UnitManager unitmanager = new UnitManager();
        public List<ItemStruct> UnitStructs;

        public UnitDimenstionModel()
        {
            UnitStructs = new List<ItemStruct>();
        }

        public List<ItemStruct> getUnitListByDimenstion(long dimensionId)
        {
            List<Unit> units = unitmanager.DimensionRepo.Get(dimensionId).Units.ToList();
            ItemStruct tempUnitStruct = new ItemStruct();
            foreach (Unit u in units)
            {
                tempUnitStruct.Id = u.Id;
                tempUnitStruct.Name = u.Name;
                UnitStructs.Add(tempUnitStruct);
            }

            return UnitStructs;
        }

        public List<ItemStruct> getUnitListByDimenstionAndDataType(long dimensionId,long dataTypeId)
        {
            List<Unit> units = unitmanager.DimensionRepo.Get(dimensionId).Units.ToList();
            ItemStruct tempUnitStruct = new ItemStruct();
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
}