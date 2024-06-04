using BExIS.Dlm.Entities.Data;
using System;

namespace BExIS.Dim.Helpers.Services
{
    public class DataCiteVersionService
    {
        public string GetVersion(DatasetVersion datasetVersion, string type, string value)
        {
            string version = null;

            switch (type)
            {
                case "Property":

                    switch (value)
                    {
                        case "Id":

                            version = Convert.ToString(datasetVersion.Id);
                            break;

                        case "Name":

                            version = datasetVersion.VersionName;
                            break;

                        case "Number":

                            version = Convert.ToString(datasetVersion.VersionNo);
                            break;
                    }

                    break;

                default:
                    break;
            }

            return version;
        }
    }
}