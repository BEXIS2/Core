using BExIS.Dlm.Entities.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.Data
{
    public class DataTupleIterator : IEnumerable<AbstractTuple>
    {
        private List<long> tupleIds = new List<long>();
        private DatasetManager datasetManager;
        private bool materialize = true;
        private int preferedBatchSize = 1;
        private long iterations = 0;

        public DataTupleIterator(List<long> tupleIds, DatasetManager datasetManager, bool materialize = true)
        {
            this.tupleIds = tupleIds;
            this.datasetManager = datasetManager;
            this.materialize = materialize;
            preferedBatchSize = datasetManager.PreferedBatchSize;
            if (tupleIds != null && tupleIds.Count > 0)
            {
                iterations = tupleIds.Count / preferedBatchSize;
                if (iterations * preferedBatchSize < tupleIds.Count)
                    iterations++;
            }
        }

        public IEnumerator<AbstractTuple> GetEnumerator()
        {
            for (int round = 0; round < iterations; round++)
            {
                datasetManager.DataTupleRepo.Evict();
                if (tupleIds.Skip(round * preferedBatchSize).Take(preferedBatchSize).Count() > 0)
                {
                    var tupleIdPack = tupleIds.Skip(round * preferedBatchSize).Take(preferedBatchSize).ToList();
                    //List<DataTuple> tuplePackage = datasetManager.DataTupleRepo.Query(d => tupleIdPack.Contains(d.Id)).ToList();
                    var uow = this.GetBulkUnitOfWork();
                    var repo = uow.GetReadOnlyRepository<DataTuple>();
                    List<DataTuple> tuplePackage = repo.Query(d => tupleIdPack.Contains(d.Id)).ToList();
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
                long iterations = tupleIds.Count / preferedBatchSize;
                if (iterations * preferedBatchSize < tupleIds.Count)
                    iterations++;

                for (int round = 0; round < iterations; round++)
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