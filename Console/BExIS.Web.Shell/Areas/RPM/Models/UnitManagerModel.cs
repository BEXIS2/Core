using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public struct EditUnitStruct
    {
        public Unit Unit {get; set;}
        public List<DataType> DataTypeList;
        public List<Dimension> DimensionList;
    }

    public class UnitManagerModel
    {
        UnitManager unitManager = new UnitManager();
        public List<Unit> UnitList = new List<Unit>();
        public EditUnitStruct editUnitStruct;

        public UnitManagerModel()
        {
            setup();

            editUnitStruct.Unit = new Unit();
            editUnitStruct.Unit.Dimension = new Dimension();
        }

        public UnitManagerModel(long UnitId)
        {
            setup();

            Unit unit = unitManager.Repo.Get(UnitId);

            editUnitStruct.Unit = unit;
            editUnitStruct.Unit.AssociatedDataTypes = unit.AssociatedDataTypes;
            editUnitStruct.Unit.Dimension = unitManager.DimensionRepo.Get(unit.Dimension.Id);

        }

        private void setup()
        {
            editUnitStruct.DimensionList = unitManager.DimensionRepo.Get().Where(d => d.Name != null && d.Specification != null).ToList();
            UnitList = unitManager.Repo.Get().Where(u => u.DataContainers.Count != null && u.AssociatedDataTypes.Count != null).ToList();
            DataTypeManager dataTypeManager = new DataTypeManager();
            editUnitStruct.DataTypeList = dataTypeManager.Repo.Get().ToList();
        
        }

    } 
}