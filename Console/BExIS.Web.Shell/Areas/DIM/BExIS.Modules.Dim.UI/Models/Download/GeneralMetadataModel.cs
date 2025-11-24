using BExIS.Modules.Dim.UI.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace BExIS.Modules.Dim.UI.Models.Download
{
    public class GeneralMetadataModel : ApiDatasetModel
    {
        //public ApiDatasetModel Dataset { get; set; }
        public DatasetDownloadInfoModel DownloadInformation { get; set; }

        public GeneralMetadataModel() : base()
        {
            DownloadInformation = new DatasetDownloadInfoModel();
        }

        public static GeneralMetadataModel Map(ApiDatasetModel source)
        {
            GeneralMetadataModel target = new GeneralMetadataModel();

            // 1. Hole alle öffentlichen Instanz-Eigenschaften des Quell-Typs
            PropertyInfo[] sourceProperties = typeof(ApiDatasetModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // 2. Durchlaufe die Eigenschaften in einer Schleife
            foreach (PropertyInfo sourceProperty in sourceProperties)
            {
                // 3. Suche die entsprechende Eigenschaft im Ziel-Typ
                PropertyInfo targetProperty = typeof(GeneralMetadataModel).GetProperty(sourceProperty.Name);

                // 4. Überprüfe, ob die Eigenschaft existiert und gesetzt werden kann
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    // 5. Überprüfe, ob die Typen übereinstimmen (empfohlen)
                    if (targetProperty.PropertyType == sourceProperty.PropertyType)
                    {
                        // 6. Lese den Wert aus der Quelle
                        object value = sourceProperty.GetValue(source);

                        // 7. Setze den Wert im Ziel
                        targetProperty.SetValue(target, value);
                    }
                }
            }

            return target;
        }
    }

    public class DatasetDownloadInfoModel
    {
        public string DownloadSource { get; set; }
        public string DownloadDate { get; set; }
        public string DownloadedBy { get; set; }
        public string DownloadedFilter { get; set; }
    }
}