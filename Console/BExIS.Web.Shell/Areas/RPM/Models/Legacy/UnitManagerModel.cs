using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class EditUnitModel
    {
        public bool inUse {get; set;}
        public Unit Unit {get; set;}
        public List<DataType> DataTypeList;
        public List<Dimension> DimensionList;

        UnitManager unitManager = new UnitManager();
        DataContainerManager dataAttributeManager = new DataContainerManager();
        DataStructureManager dataStructureManager = new DataStructureManager();
        DataTypeManager dataTypeManager = new DataTypeManager();

        public EditUnitModel()
        {
            bool inUse = false;
            Unit = new Unit();
            Unit.Dimension = new Dimension();
            setup();
            
        }

        public EditUnitModel(Unit unit)
        {
            setup(unit.Id);
        }

        public EditUnitModel(long unitId)
        {
            setup(unitId);
        }

        private void setup()
        {
            DataTypeList = dataTypeManager.Repo.Get().ToList();
            DimensionList = unitManager.DimensionRepo.Get().ToList();
        }

        private void setup(long unitId)
        {
            setup();
            Unit tempUnit = unitManager.Repo.Get().Where(u => u.Id.Equals(unitId) && u.AssociatedDataTypes.Count() != null).FirstOrDefault();
            Unit = tempUnit;
            if (tempUnit.AssociatedDataTypes.Count != 0)
                Unit.AssociatedDataTypes = tempUnit.AssociatedDataTypes;
            else
                Unit.AssociatedDataTypes = new List<DataType>();

            Unit.Dimension = unitManager.DimensionRepo.Get(tempUnit.Dimension.Id);
            inUse = unitInUse(tempUnit);
        }

        private bool unitInUse(Unit unit)
        {
            bool inUse = false;
            if (unit.Name.ToLower() == "none")
                inUse = true;
            else if (dataAttributeManager.DataAttributeRepo.Query(d => d.Unit.Id.Equals(unit.Id)).Count() > 0)
                inUse = true;
            else if (dataStructureManager.VariableRepo.Query(d => d.Unit.Id.Equals(unit.Id)).Count() > 0)
                inUse = true;
            return inUse;
        }
    }

    public class UnitManagerModel
    {
        UnitManager unitManager = new UnitManager();
        public List<EditUnitModel> editUnitModelList = new List<EditUnitModel>();

        public EditUnitModel editUnitModel;

        public UnitManagerModel()
        {
            setup();
            editUnitModel = new EditUnitModel();
        }

        public UnitManagerModel(long unitId)
        {
            setup();
            editUnitModel = new EditUnitModel(unitId);

        }

        private void setup()
        {
            List<Unit> tempUnitList = unitManager.Repo.Get().ToList();
            foreach (Unit u in tempUnitList)
            { 
                editUnitModelList.Add(new EditUnitModel(u));
            }
        }

    } 
}