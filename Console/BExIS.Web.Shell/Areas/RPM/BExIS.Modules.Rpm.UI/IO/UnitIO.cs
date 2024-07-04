using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using System.Linq;

namespace BExIS.Modules.Rpm.UI.Classes
{
    public static class UnitIO
    {
        public static Unit getUnit(long unitId)
        {
            if (unitId > 0)
            {
                UnitManager unitManager = null;
                try
                {
                    unitManager = new UnitManager();
                    return unitManager.Repo.Get(unitId);
                }
                finally
                {
                    unitManager.Dispose();
                }
            }
            return null;
        }

        public static Unit ceateUnit(Unit unit)
        {
            if (unit != null && unit.Id != 0)
            {
                UnitManager unitManager = null;
                try
                {
                    unitManager = new UnitManager();
                    return unitManager.Create(unit.Name, unit.Abbreviation, unit.Description, unit.Dimension, unit.MeasurementSystem);
                }
                finally
                {
                    unitManager.Dispose();
                }
            }
            return null;
        }

        public static Unit edidUnit(Unit unit)
        {
            if (unit != null && unit.Id != 0 && unit.DataContainers.ToList().Count() <= 0)
            {
                UnitManager unitManager = null;
                try
                {
                    unitManager = new UnitManager();
                    return unitManager.Update(unit);
                }
                finally
                {
                    unitManager.Dispose();
                }
            }
            return null;
        }

        public static bool deletUnit(Unit unit)
        {
            if (unit != null && unit.Id != 0 && unit.DataContainers.ToList().Count() <= 0)
            {
                UnitManager unitManager = null;
                try
                {
                    unitManager = new UnitManager();
                    return unitManager.Delete(unit);
                }
                finally
                {
                    unitManager.Dispose();
                }
            }
            return false;
        }
    }
}