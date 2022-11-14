namespace BExIS.Modules.Rpm.UI.Models
{
    public class UnitDimenstionModel
    {
        UnitManager unitmanager = null;
        public List<ItemStruct> UnitStructs;

        public UnitDimenstionModel()
        {
            UnitStructs = new List<ItemStruct>();
        }

        public List<ItemStruct> getUnitListByDimenstion(long dimensionId)
        {
            try
            {
                unitmanager = new UnitManager();
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
            finally
            {
                unitmanager.Dispose();
            }
        }

        public List<ItemStruct> getUnitListByDimenstionAndDataType(long dimensionId, long dataTypeId)
        {
            try
            {
                unitmanager = new UnitManager();
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
            finally
            {
                unitmanager.Dispose();
            }

        }
    }
}