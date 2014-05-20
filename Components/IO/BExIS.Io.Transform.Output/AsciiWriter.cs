using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Io.Transform.Output
{
    public class AsciiWriter:DataWriter
    {

        public string CreateFile(long datasetId, long datasetVersionOrderNr, long dataStructureId, string title, string extention)
        {
            string dataPath = GetStorePath(datasetId, datasetVersionOrderNr, title, extention);



            return dataPath;
        }



    }
}
