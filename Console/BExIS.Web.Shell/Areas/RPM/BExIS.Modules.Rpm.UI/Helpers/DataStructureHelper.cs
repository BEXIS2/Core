using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Models.DataStructure;
using Sylvan.Data.Csv;
using Sylvan.Data.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Vaiona.Utils.Cfg;

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

        public string ConvertExcelToCsv()
        {
            string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Template");
            string excel = Path.Combine(path, "BExISppTemplate_Clean.xlsx");
            string csv = Path.Combine(path, "data.csv");

            using (var excelReader = ExcelDataReader.Create(excel))
            using (var csvWriter = CsvDataWriter.Create(csv))
            {
                csvWriter.Write(excelReader);
            }

            return "";
        }

        public List<MissingValue> ConvertTo(List<MissingValueModel> MissingValues)
        {
            List<MissingValue> list = new List<MissingValue>();
            foreach (var mv in MissingValues)
            {
                list.Add(new MissingValue()
                {
                    DisplayName = mv.DisplayName,
                    Description = mv.Description,
                });
            }

            return list;
        }
    }
}