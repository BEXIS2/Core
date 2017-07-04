using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Xml.Helpers.Mapping;
using Vaiona.Utils.Cfg;

namespace BExIS.IO.Transform.Output
{
    public class OutputDataStructureManager
    {
        /// <summary>
        /// generate a text file with JSON from a datastructure of a dataset
        /// and stored this file on the server
        /// and store the path in the content discriptor
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns>dynamic filepath</returns>
        public static string GenerateDataStructure(long datasetId)
        {
            string path = "";
            try
            {
                DatasetManager datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);
                DataStructureManager dataStructureManager = new DataStructureManager();

                long dataStructureId = datasetVersion.Dataset.DataStructure.Id;
                DataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                if (dataStructure != null)
                {

                }

                // store in content descriptor
                path = storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, "datastructure", ".txt");

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return path;
        }

     
        private static string storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion, string title, string ext)
        {

            string name = "";
            string mimeType = "";

            if (ext.Contains("csv"))
            {
                name = "datastructure";
                mimeType = "text/comma-separated-values";
            }


            // create the generated FileStream and determine its location
            string dynamicPath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, datasetVersion.Id, title, ext);
            //Register the generated data FileStream as a resource of the current dataset version
            //ContentDescriptor generatedDescriptor = new ContentDescriptor()
            //{
            //    OrderNo = 1,
            //    Name = name,
            //    MimeType = mimeType,
            //    URI = dynamicPath,
            //    DatasetVersion = datasetVersion,
            //};

            DatasetManager dm = new DatasetManager();
            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name)) > 0)
            {   // remove the one contentdesciptor 
                foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                {
                    if (cd.Name == name)
                    {
                        cd.URI = dynamicPath;
                        dm.UpdateContentDescriptor(cd);
                    }
                }
            }
            else
            {
                // add current contentdesciptor to list
                //datasetVersion.ContentDescriptors.Add(generatedDescriptor);
                dm.CreateContentDescriptor(name, mimeType, dynamicPath, 1, datasetVersion);
            }

            //dm.EditDatasetVersion(datasetVersion, null, null, null);
            return dynamicPath;
        }

    }
}
