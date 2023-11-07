using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Rpm.UI.Helpers
{
    public class DataStructureHelper
    {
        public bool InUseAndDataExist(long structureId)
        {
            using (var datasetManager = new DatasetManager())
            using (var structureManager = new DataStructureManager())
            {
                var dataStructure = structureManager.StructuredDataStructureRepo.Get(structureId);
                if (dataStructure == null) return false;

                foreach (Dataset d in dataStructure.Datasets)
                {
                    if (datasetManager.RowCount(d.Id, null) > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}