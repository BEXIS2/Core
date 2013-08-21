using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;

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