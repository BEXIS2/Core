using System.Data;

namespace BExIS.Modules.Dim.UI.Models
{
    /// <summary>
    /// It is only a wrapper around the DataTable class to provide a specific name for Result formatting in MVC pipeline.
    /// </summary>
    public class DatasetModel
    {
        public DataTable DataTable { get; set; }

        public DatasetModel()
        {
            DataTable = new DataTable("empty");
        }
    }
}