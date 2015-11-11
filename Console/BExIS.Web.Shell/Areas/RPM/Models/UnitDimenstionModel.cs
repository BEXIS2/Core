using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public struct UnitStruct
    {
        public long Id {get; set;}
        public string Name {get; set;}
    }

    public class UnitDimenstionModel
    {
        UnitManager unitmanager = new UnitManager();
        public List<UnitStruct> UnitStructs;

        public UnitDimenstionModel()
        {
            UnitStructs = new List<UnitStruct>();
        }

        public List<UnitStruct> getUnitListByDimenstion(long dimensionId)
        {
            List<Unit> units = unitmanager.DimensionRepo.Get(dimensionId).Units.ToList();
            UnitStruct tempUnitStruct = new UnitStruct();
            foreach (Unit u in units)
            {
                tempUnitStruct.Id = u.Id;
                tempUnitStruct.Name = u.Name;
                UnitStructs.Add(tempUnitStruct);
            }

            return UnitStructs;
        }

        public List<UnitStruct> getUnitListByDimenstionAndDataType(long dimensionId,long dataTypeId)
        {
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
}