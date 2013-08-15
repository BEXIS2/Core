using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Data;
using System.Diagnostics.Contracts;
using Vaiona.Persistence.Api;

namespace BExIS.Dlm.Services.DataStructure
{
    public class ViewManager
    {
        //Create for dataset, create for datastrcuture, promote for data strcuture
        // create takes a nullable Dataset as parameter
        public DataView CreateDataView(string name, string contentSelectionCriterion, string containerSelectionCriterion, Dataset dataset)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(contentSelectionCriterion) || !string.IsNullOrWhiteSpace(containerSelectionCriterion));
            Contract.Requires(dataset != null);
            Contract.Ensures(Contract.Result<DataView>() != null);

            DataView e = new DataView()
            {
                Name = name,
                ContentSelectionCriterion = contentSelectionCriterion,
                ContainerSelectionCriterion = containerSelectionCriterion,
                Dataset = dataset,
            };
            e.Dataset.Views.Add(e);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataView> repo = uow.GetRepository<DataView>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public DataView CreateDataView(string name, string contentSelectionCriterion, string containerSelectionCriterion, BExIS.Dlm.Entities.DataStructure.DataStructure dataStructure)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(!string.IsNullOrWhiteSpace(contentSelectionCriterion) || !string.IsNullOrWhiteSpace(containerSelectionCriterion));
            Contract.Requires(dataStructure != null);
            Contract.Ensures(Contract.Result<DataView>() != null);

            DataView e = new DataView()
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
                IRepository<DataView> repo = uow.GetRepository<DataView>();
                repo.Put(e);
                uow.Commit();
            }
            return (e);
        }

        public bool DeleteDataView(DataView entity)
        {
            Contract.Requires(entity != null);
            Contract.Requires(entity.Id >= 0);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataView> repo = uow.GetRepository<DataView>();

                entity = repo.Reload(entity);
                entity.Dataset = null;
                entity.DataStructures.Clear();

                repo.Delete(entity);

                uow.Commit();
            }
            return (true);
        }

        public bool DeleteDataView(IEnumerable<DataView> entities)
        {
            Contract.Requires(entities != null);
            Contract.Requires(Contract.ForAll(entities, (DataView e) => e != null));
            Contract.Requires(Contract.ForAll(entities, (DataView e) => e.Id >= 0));

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataView> repo = uow.GetRepository<DataView>();

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

        public DataView UpdateDataView(DataView entity)
        {
            Contract.Requires(entity != null, "provided entity can not be null");
            Contract.Requires(entity.Id >= 0, "provided entity must have a permant ID");

            Contract.Ensures(Contract.Result<DataView>() != null && Contract.Result<DataView>().Id >= 0, "No entity is persisted!");

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataView> repo = uow.GetRepository<DataView>();
                repo.Put(entity); // Merge is required here!!!!
                uow.Commit();
            }
            return (entity);
        }
      
    }
}
