using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;

namespace BExIS.IO.Transform.Input
{
    public class APIDataReader : DataReader
    {
        public APIDataReader(StructuredDataStructure structuredDatastructure, ApiFileReaderInfo fileReaderInfo) : base(structuredDatastructure, fileReaderInfo)
        {
        }

        public APIDataReader(StructuredDataStructure structuredDatastructure, ApiFileReaderInfo fileReaderInfo, IOUtility iOUtility) : base(structuredDatastructure, fileReaderInfo, iOUtility)
        {
        }

        public APIDataReader(StructuredDataStructure structuredDatastructure, ApiFileReaderInfo fileReaderInfo, IOUtility iOUtility, DatasetManager datasetManager) : base(structuredDatastructure, fileReaderInfo, iOUtility, datasetManager)
        {
        }
    }
}