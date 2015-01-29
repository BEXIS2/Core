using System.Collections.Generic;
using System.IO;
using System.Web;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class UploadWizardParameters
    {
        public Stream fileStream;
        public int datasetID;
        public int datastructureID;
        public bool isTemplate;

        public HttpPostedFileBase uploadFileBase;

        public List<int> selectableDatasetIds;
        public List<int> selectableDatastructureIds;

        public List<Error> ErrorList;

        public List<DataTuple> DataTuples;

        public UploadWizardParameters()
        {
            DataTuples = new List<DataTuple>();
        }


    }
}