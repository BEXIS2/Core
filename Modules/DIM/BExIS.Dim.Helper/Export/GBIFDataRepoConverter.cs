using BExIS.Dim.Entities.Publication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Helpers.Export
{
    public class GBIFDataRepoConverter : IDataRepoConverter
    {
        private Repository _dataRepo { get; set; }
        private Broker _broker { get; set; }

        public string Convert(long datasetVersionId)
        {
            throw new NotImplementedException();
        }

        public bool Validate(long datasetVersionId)
        {
            throw new NotImplementedException();
        }

        public GBIFDataRepoConverter(Repository datarepo)
        {
            _dataRepo = datarepo;
            _broker = datarepo.Broker;
        }
    }
}
