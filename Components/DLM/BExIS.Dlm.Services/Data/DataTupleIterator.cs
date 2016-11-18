using BExIS.Dlm.Entities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dlm.Services.Data
{
    public class DataTupleIterator : IEnumerable<AbstractTuple> 
    {
        List<long> tupleIds = new List<long>();
        DatasetManager datasetManager;
        bool materialize = true;
        int preferedBatchSize = 1;
        long iternations = 0;
        public DataTupleIterator(List<long> tupleIds, DatasetManager datasetManager, bool materialize = true)
        {
            this.tupleIds = tupleIds;
            this.datasetManager = datasetManager;
            this.materialize = materialize;
            preferedBatchSize = datasetManager.PreferedBatchSize;
            if (tupleIds != null && tupleIds.Count > 0)
            {
                iternations = tupleIds.Count / preferedBatchSize;
                if (iternations * preferedBatchSize < tupleIds.Count)
                    iternations++;
            }
        }
        public IEnumerator<AbstractTuple> GetEnumerator()
        {
            for (int round = 0; round < iternations; round++)
            {
                datasetManager.DataTupleRepo.Evict();
                if (tupleIds.Skip(round * preferedBatchSize).Take(preferedBatchSize).Count() > 0)
                {
                    var tupleIdPack = tupleIds.Skip(round * preferedBatchSize).Take(preferedBatchSize).ToList();
                    List<DataTuple> tuplePackage = datasetManager.DataTupleRepo.Query(d => tupleIdPack.Contains(d.Id)).ToList();
                    foreach (var dataTuple in tuplePackage)
                    {
                        if (materialize)
                            dataTuple.Materialize();
                        yield return dataTuple;
                    }
                    tuplePackage.Clear();
                    tuplePackage = null;
                }
                GC.Collect();
            }
            yield break;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private IEnumerator<DataTuple> iterate()
        {
            int preferedBatchSize = datasetManager.PreferedBatchSize;
            if (tupleIds != null && tupleIds.Count > 0)
            {
                long iternations = tupleIds.Count / preferedBatchSize;
                if (iternations * preferedBatchSize < tupleIds.Count)
                    iternations++;

                for (int round = 0; round < iternations; round++)
                {
                    datasetManager.DataTupleRepo.Evict();
                    if (tupleIds.Skip(round * preferedBatchSize).Take(preferedBatchSize).Count() > 0)
                    {
                        var tupleIdPack = tupleIds.Skip(round * preferedBatchSize).Take(preferedBatchSize).ToList();
                        List<DataTuple> tuplePackage = datasetManager.DataTupleRepo.Query(d => tupleIdPack.Contains(d.Id)).ToList();
                        foreach (var dataTuple in tuplePackage)
                        {
                            if (materialize)
                                dataTuple.Materialize();
                            yield return dataTuple;
                        }
                        tuplePackage.Clear();
                        tuplePackage = null;
                    }
                    GC.Collect();
                }
            }
            yield break;
        }


    }
}
