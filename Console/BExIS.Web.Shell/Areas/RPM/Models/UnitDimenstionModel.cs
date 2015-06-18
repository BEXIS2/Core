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
        public Dimension Dimension {get; set;}
    }

    public class UnitDimenstionModel
    {   
        List<UnitStruct> unitStructs = new List<UnitStruct>();

        public UnitDimenstionModel()
        { 
            UnitManager unitManager = new UnitManager();
            List<Unit> units = unitManager.Repo.Get().ToList();
            UnitStruct unitStruct = new UnitStruct();
            for (int i = 0; i < units.Count; i++)
            {
                unitStruct.Id = units.ElementAt(i).Id;
                unitStruct.Name = units.ElementAt(i).Name;
                unitStruct.Dimension = units.ElementAt(i).Dimension;
                this.unitStructs.Add(unitStruct);
            }
        }

        public List<UnitStruct> getUnitDimenstionListByDimenstion(Dimension dimension)
        {
            List<UnitStruct> tempUnitStructs = new List<UnitStruct>();

            for (int i = 0; i < unitStructs.Count; i++)
            {
                if (unitStructs.ElementAt(i).Dimension.Specification.Trim().ToLower() == dimension.Specification.Trim().ToLower())
                {
                    tempUnitStructs.Add(unitStructs.ElementAt(i));
                }
            }
            return tempUnitStructs;
        }

    }
}