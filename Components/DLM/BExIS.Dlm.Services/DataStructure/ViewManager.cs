using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public class ViewManager
    {
        //Create for dataset, create for datastrcuture, promote for data strcuture
        // create takes a nullable Dataset as parameter
        public DatasetView CreateDataView(string name, string contentSelectionCriterion, string containerSelectionCriterion, Dataset dataset)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(contentSelectionCriterion) || !string.IsNullOrWhiteSpace(containerSelectionCriterion));
            Contract.Requires(dataset != null);
            Contract.Ensures(Contract.Result<DatasetView>() != null);

            DatasetView e = new DatasetView()
            {
                Name = name,
                ContentSelectionCriterion = contentSelectionCriterion,
                ContainerSelectionCriterion = containerSelectionCriterion,
                Dataset = dataset,
            };
            e.Dataset.Views.Add(e);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DatasetView> repo = uow.GetRepository<DatasetView>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public DatasetView CreateDataView(string name, string contentSelectionCriterion, string containerSelectionCriterion, BExIS.Dlm.Entities.DataStructure.DataStructure dataStructure)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(contentSelectionCriterion) || !string.IsNullOrWhiteSpace(containerSelectionCriterion));
            Contract.Requires(dataStructure != null);
            Contract.Ensures(Contract.Result<DatasetView>() != null);

            DatasetView e = new DatasetView()
            {
                Name = name,
                ContentSelectionCriterion = contentSelectionCriterion,
                ContainerSelectionCriterion = containerSelectionCriterion,
                Dataset = null,
            };
            dataStructure.Views.Add(e);
            e.DataStructures.Add(dataStructure);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                // maybe there is a need for persisting the data structure also!
                IRepository<DatasetView> repo = uow.GetRepository<DatasetView>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool DeleteDataView(DatasetView entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DatasetView> repo = uow.GetRepository<DatasetView>();

                entity = repo.Reload(entity);
                entity.Dataset = null;
                entity.DataStructures.Clear();

                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        public bool DeleteDataView(IEnumerable<DatasetView> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (DatasetView e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (DatasetView e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DatasetView> repo = uow.GetRepository<DatasetView>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    latest.Dataset = null;
                    latest.DataStructures.Clear();

                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        public DatasetView UpdateDataView(DatasetView entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<DatasetView>() != null && Contract.Result<DatasetView>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DatasetView> repo = uow.GetRepository<DatasetView>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
            return (entity);
        }
    }
}