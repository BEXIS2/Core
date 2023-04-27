using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using Vaelastrasz.Library.Models;

namespace BExIS.Modules.Dim.UI.Models
{
    public class CreateDataCiteDoiModel
    {
        public long DatasetId { get; set; }
        public long DatasetVersionId { get; set; }
        public CreateDataCiteModel DataCiteModel { get; set; }
        public string JSON => JsonConvert.SerializeObject(DataCiteModel, Formatting.Indented);

        public CreateDataCiteDoiModel()
        {
            DatasetId = 0;
            DatasetVersionId = 0;
            DataCiteModel = new CreateDataCiteModel();
        }

        public CreateDataCiteDoiModel(long datasetId, long datasetVersionId)
        {
            DatasetId = datasetId;
            DatasetVersionId = datasetVersionId;
            DataCiteModel = new CreateDataCiteModel();

        }

        public CreateDataCiteDoiModel(long datasetId, long datasetVersionId, CreateDataCiteModel dataCiteModel)
        {
            DatasetId = datasetId;
            DatasetVersionId = datasetVersionId;
            DataCiteModel = dataCiteModel;
        }
    }

    public class ReadDataCiteDoiModel
    {

    }

    public class UpdateDataCiteDoiModel
    {

    }
}