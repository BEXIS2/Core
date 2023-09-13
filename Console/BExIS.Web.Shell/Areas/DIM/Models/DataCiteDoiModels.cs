using Newtonsoft.Json;
using Vaelastrasz.Library.Models;

namespace BExIS.Modules.Dim.UI.Models
{
    public class CreateDataCiteDOIModel
    {
        public CreateDataCiteDOIModel()
        {
            DatasetId = 0;
            DatasetVersionId = 0;
            DataCiteModel = new CreateDataCiteModel();
        }

        public CreateDataCiteDOIModel(long datasetId, long datasetVersionId)
        {
            DatasetId = datasetId;
            DatasetVersionId = datasetVersionId;
            DataCiteModel = new CreateDataCiteModel();
        }

        public CreateDataCiteDOIModel(long datasetId, long datasetVersionId, CreateDataCiteModel dataCiteModel)
        {
            DatasetId = datasetId;
            DatasetVersionId = datasetVersionId;
            DataCiteModel = dataCiteModel;
        }

        public Vaelastrasz.Library.Models.CreateDataCiteModel DataCiteModel { get; set; }
        public long DatasetId { get; set; }
        public long DatasetVersionId { get; set; }
        public string JSON => JsonConvert.SerializeObject(DataCiteModel, Formatting.Indented);
    }

    public class ReadDataCiteDOIModel
    {
    }

    public class UpdateDataCiteDOIModel
    {
    }
}