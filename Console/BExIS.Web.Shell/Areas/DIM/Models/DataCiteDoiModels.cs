using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vaelastrasz.Library.Models;

namespace BExIS.Modules.Dim.UI.Models
{
    public class CreateDataCiteDoiModel
    {
        public long DatasetId { get; set; }
        public long DatasetVersionId { get; set; }
        public CreateDataCiteModel DataCiteModel { get; set; }

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